using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Server.Protocols;

namespace Carbon.Match.New.Room
{
    public class EmulatorRoom : IEmulatorRoom, IDisposable
    {
        /// <summary>
        /// 房间类型
        /// </summary>
        public string RoomType { get; set; }
        /// <summary>
        /// PVP-房间号
        /// </summary>
        public long CtrlRoomId { get; set; }
        /// <summary>
        /// 主场玩家ID
        /// </summary>
        public long Home { get; set; }
        /// <summary>
        /// 客场玩家ID
        /// </summary>
        public long Away { get; set; }
        /// <summary>
        /// 主场
        /// </summary>
        public RoomPlayer? HomePlayer { get; set; }
        /// <summary>
        /// 客场
        /// </summary>
        public RoomPlayer? AwayPlayer { get; set; }
        /// <summary>
        /// 战斗服房间id
        /// </summary>
        public string RoomId { get; set; }
        /// <summary>
        /// 主服务器的地址
        /// </summary>
        public string CallbackUrl { get; set; }
        /// <summary>
        /// 引用的房间管理器
        /// </summary>
        private readonly IRoomManager roomManager;

        private readonly ConcurrentQueue<PvPMessage> homeMessages = new();
        private readonly ConcurrentQueue<PvPMessage> awayMessages = new();

        private const int ScheduleIntervalMs = 100;
        private const int ForceShutdownCount = 60 * 1000 / ScheduleIntervalMs;
        private readonly object lockable = new();
        private readonly ILogger logger;

        private bool shutdown;
        private bool done;
        private bool disposed;

        public EmulatorRoom(IRoomManager rManager, string roomType, long roomId, long home, long away, string callbackUrl, ILogger logger)
        {
            this.CtrlRoomId = roomId;
            this.RoomType = roomType;
            this.Home = home;
            this.Away = away;
            this.RoomId = Guid.NewGuid().ToString();
            this.roomManager = rManager;
            this.CallbackUrl = callbackUrl;
            this.logger = logger;

            (new Task(async () => await this.RunAsync())).Start();
        }

        /// <summary>
        /// 玩家进来
        /// </summary>
        /// <param name="player"></param>
        public void PlayerEnter(RoomPlayer player)
        {
            if (player.Pid == this.Home)
            {
                this.HomePlayer = player;
            }
            if (player.Pid == this.Away)
            {
                this.AwayPlayer = player;
            }
        }

        /// <summary>
        /// 玩家离开
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="context"></param>
        public void PlayerLeave(long pid, HttpContext context)
        {
            if (pid == this.Home && this.HomePlayer != null && this.HomePlayer.Context == context)
            {
                this.HomePlayer = null;
            }
            else if (pid == this.Away && this.AwayPlayer != null && this.AwayPlayer.Context == context)
            {
                this.AwayPlayer = null;
            }
            else
            {
                this.logger.LogWarning($"player {pid} fail to leave  by connection {context.Connection.Id}");
            }
        }

        /// <summary>
        /// 玩家是否属于该房间
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public bool CheckPlayerExists(long pid) => this.Home == pid || this.Away == pid;

        /// <summary>
        /// 消息来了
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="context"></param>
        /// <param name="pvpMesage"></param>
        public void QueueMessage(long pid, HttpContext context, PvPMessage pvpMesage)
        {
            if (!this.disposed)
            {
                if (pid == this.Home && this.HomePlayer != null && this.HomePlayer.Context == context)
                {
                    this.homeMessages.Enqueue(pvpMesage);
                }
                else if (pid == this.Away && this.AwayPlayer != null && this.AwayPlayer.Context == context)
                {
                    this.awayMessages.Enqueue(pvpMesage);
                }
                else
                {
                    this.logger.LogWarning($"player {pid} fail to queue message due to {context.Connection.Id}:{this.HomePlayer.Context.Connection.Id}:{this.AwayPlayer.Context.Connection.Id}");
                }
            }
        }

        /// <summary>
        /// 主循环
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task RunAsync()
        {
            var idle = 0;
            try
            {
                while (true)
                {
                    if (this.disposed || this.done)
                    {
                        this.logger.LogDebug($"Room roomId={this.RoomId} is done from task flags disposed={this.disposed} done={this.done}.");
                        break;
                    }

                    if (this.homeMessages.IsEmpty && this.awayMessages.IsEmpty)
                    {
                        idle++;
                        if (idle > ForceShutdownCount)
                        {
                            if (this.shutdown)
                            {
                                this.logger.LogDebug($"Room roomId={this.RoomId} is ready for shutdown.");
                                break;
                            }
                            else
                            {
                                //this.room.Abort();
                                this.shutdown = true;
                                idle = 0;
                                this.logger.LogDebug($"Room roomId={this.RoomId} is shutdowning due to idle.");
                            }
                        }
                    }
                    else
                    {
                        idle = 0;
                    }

                    var startTime = DateTime.Now;
                    while (!this.homeMessages.IsEmpty)
                    {
                        if (this.homeMessages.TryDequeue(out var message))
                        {
                            await this.SendMessageAsync(this.Away, message);
                        }
                    }
                    while (!this.awayMessages.IsEmpty)
                    {
                        if (this.awayMessages.TryDequeue(out var message))
                        {
                            await this.SendMessageAsync(this.Home, message);
                        }
                    }

                    var delayMs = ScheduleIntervalMs - (int)(DateTime.Now - startTime).TotalMilliseconds;
                    if (delayMs > 0)
                    {
                        await Task.Delay(delayMs);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Room roomId={this.RoomId} catched exception {ex.Message}.");
            }
            finally
            {
                this.roomManager.DestoryRoom(this.RoomId);
                this.logger.LogDebug($"Room roomId={this.RoomId} is finishing in try-catch-finaly.");
            }
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="pvpMessage"></param>
        public async Task SendMessageAsync(long pid, PvPMessage pvpMessage)
        {
            if (pid == this.Home && this.HomePlayer != null)
            {
                await this.HomePlayer.SendAsync(pvpMessage, this.logger);
            }
            else if (pid == this.Away && this.AwayPlayer != null)
            {
                await this.AwayPlayer.SendAsync(pvpMessage, this.logger);
            }
            else
            {
                this.logger.LogWarning($"Player {pid} fail to send message due to {this.Home}:{this.HomePlayer != null} {this.Away}:{this.AwayPlayer != null}");
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.Home = 0;
                this.Away = 0;
                this.homeMessages.Clear();
                this.awayMessages.Clear();
                GC.SuppressFinalize(this);
            }
        }
    }
}
