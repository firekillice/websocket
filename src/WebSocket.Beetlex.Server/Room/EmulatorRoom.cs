using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BeetleX;
using BeetleX.EventArgs;
using Carbon.Match.Message;
using Carbon.Match.Utility;
using Server.Protocols;

namespace Carbon.Match.Room
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

        private bool shutdown;
        private bool done;
        private bool disposed;

        public EmulatorRoom(IRoomManager rManager, string roomType, long roomId, long home, long away, string callbackUrl)
        {
            this.CtrlRoomId = roomId;
            this.RoomType = roomType;
            this.Home = home;
            this.Away = away;
            this.RoomId = Guid.NewGuid().ToString();
            this.roomManager = rManager;
            this.CallbackUrl = callbackUrl;

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
        /// <param name="session"></param>
        public void PlayerLeave(long pid, ISession session)
        {
            if (pid == this.Home && this.HomePlayer != null && this.HomePlayer.Session == session)
            {
                this.HomePlayer = null;
            }
            else if (pid == this.Away && this.AwayPlayer != null && this.AwayPlayer.Session == session)
            {
                this.AwayPlayer = null;
            }
            else
            {
                LogUtility.Log(LogType.Warring, "Player {0} fail to leave  by session {1} due to home:{2} homeExists:{3} homeSession:{4} away:{5} awayExists:{6} awaySession:{7} ",
                    pid,
                    session.ID,
                    this.Home, this.HomePlayer != null, this.HomePlayer?.Session?.ID ?? 0,
                    this.Away, this.AwayPlayer != null, this.AwayPlayer?.Session?.ID ?? 0);
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
        /// <param name="session"></param>
        /// <param name="pvpMesage"></param>
        public void QueueMessage(long pid, ISession session, PvPMessage pvpMesage)
        {
            if (!this.disposed)
            {
                if (pid == this.Home && this.HomePlayer != null && this.HomePlayer.Session == session)
                {
                    this.homeMessages.Enqueue(pvpMesage);
                }
                else if (pid == this.Away && this.AwayPlayer != null && this.AwayPlayer.Session == session)
                {
                    this.awayMessages.Enqueue(pvpMesage);
                }
                else
                {
                    LogUtility.Log(LogType.Warring, "Player {0} fail to queue message by session {1} due to home:{2} homeExists:{3} homeSession:{4} away:{5} awayExists:{6} awaySession:{7}",
                        pid,
                        session.ID,
                        this.Home, this.HomePlayer != null, this.HomePlayer?.Session?.ID ?? 0,
                        this.Away, this.AwayPlayer != null, this.AwayPlayer?.Session?.ID ?? 0);
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
                        LogUtility.Log(LogType.Debug, "Room roomId={0} is done from task flags disposed={1} done={2}.", this.RoomId, this.disposed, this.done);
                        break;
                    }

                    if (this.homeMessages.IsEmpty && this.awayMessages.IsEmpty)
                    {
                        idle++;
                        if (idle > ForceShutdownCount)
                        {
                            if (this.shutdown)
                            {
                                LogUtility.Log(LogType.Debug, "Room roomId={0} is ready for shutdown.", this.RoomId);
                                break;
                            }
                            else
                            {
                                this.shutdown = true;
                                idle = 0;
                                LogUtility.Log(LogType.Info, "Room roomId={0} is shutdowning due to idle.", this.RoomId);
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

                            this.SendMessage(this.Away, message);
                        }
                    }
                    while (!this.awayMessages.IsEmpty)
                    {
                        if (this.awayMessages.TryDequeue(out var message))
                        {
                            this.SendMessage(this.Home, message);
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
                LogUtility.Log(LogType.Error, "Room roomId={0} catched exception {1}.", this.RoomId, ex.Message);
            }
            finally
            {
                this.roomManager.DestoryRoom(this.RoomId);
                LogUtility.Log(LogType.Debug, "Room roomId={0} is finishing in try-catch-finaly.", this.RoomId);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="pvpMessage"></param>
        public void SendMessage(long pid, PvPMessage pvpMessage)
        {
            if (pid == this.Home && this.HomePlayer != null)
            {
                this.HomePlayer.Send(pvpMessage);
            }
            else if (pid == this.Away && this.AwayPlayer != null)
            {
                this.AwayPlayer.Send(pvpMessage);
            }
            else
            {
                LogUtility.Log(LogType.Warring, "Player {0} fail to send message due to home:{1} homeExists:{2} away:{3} awayExists:{4}", pid, this.Home, this.HomePlayer != null, this.Away, this.AwayPlayer != null);
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
