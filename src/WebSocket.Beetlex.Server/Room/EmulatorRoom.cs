using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BeetleX;
using BeetleX.EventArgs;
using Carbon.Core.Contracts.Match;
using Carbon.Core.Shared.Match;
using Carbon.Match.Contact;
using Carbon.Match.Message;
using Carbon.Match.Utility;
using Carbon.PvP.Core;
using Carbon.PvP.Networking;
using Orleans;

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

        private readonly GameContext room;

        private readonly ConcurrentQueue<PVPMessage> homeMessages = new();
        private readonly ConcurrentQueue<PVPMessage> awayMessages = new();

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
            this.room = new();

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
        public void QueueMessage(long pid, ISession session, PVPMessage pvpMesage)
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
                                this.room.Abort();
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
                            var (b, sequenceNo, responseSequenceNo) = MessageConverter.Read(message);
                            this.room.EnqueueIncomingMessage(ConnectionRole.Home, b, sequenceNo, responseSequenceNo);

                            //this.SendMessage(this.Away, message);

                            //LogUtility.Log(LogType.Debug, "Room roomId={0} is handling message from {1}, reset {2}.", this.RoomId, this.Home, this.homeMessages.Count);
                        }
                    }
                    while (!this.awayMessages.IsEmpty)
                    {
                        if (this.awayMessages.TryDequeue(out var message))
                        {
                            var (b, sequenceNo, responseSequenceNo) = MessageConverter.Read(message);
                            this.room.EnqueueIncomingMessage(ConnectionRole.Away, b, sequenceNo, responseSequenceNo);

                            //this.SendMessage(this.Home, message);
                            //LogUtility.Log(LogType.Debug, "Room roomId={0} is handling message from {1} msg reset {2}.", this.RoomId, this.Away, this.awayMessages.Count);
                        }
                    }

                    this.room.Ready();

                    while (this.room.TryDequeueOutgoingMessage(out var role, out var b, out var sequenceNo, out var responseSequenceNo))
                    {
                        var playerId = role == ConnectionRole.Home ? this.Home : this.Away;
                        try
                        {
                            this.SendMessage(playerId, MessageConverter.Write(b, sequenceNo, responseSequenceNo));
                        }
                        catch (Exception ex)
                        {
                            LogUtility.Log(LogType.Error, "Room roomId={0} failed to send message to {1} due to exception {2}.", this.RoomId, playerId, ex.Message);
                        }
                    }

                    while (this.room.TryDequeueEvent(out var @event))
                    {
                        if (@event is MatchEndEvent match)
                        {
                            LogUtility.Log(LogType.Debug, "Room roomId={0} received match-done from inner event", this.RoomId);
                            this.TryMatchDone(match.Match);
                        }
                        else
                        {
                            LogUtility.Log(LogType.Warring, "Room roomId={0} receive unexpected evnet type {1}.", this.RoomId, @event);
                        }
                    }

                    this.room.Advance();

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
                this.TryMatchDone(null);
                this.roomManager.DestoryRoom(this.RoomId);
                LogUtility.Log(LogType.Debug, "Room roomId={0} is finishing in try-catch-finaly.", this.RoomId);
            }
        }

        /// <summary>
        /// 尝试通知逻辑服，避免重复，使用独立Task运行
        /// </summary>
        /// <param name="match"></param>
        private void TryMatchDone(Carbon.Protocols.Match? match)
        {
            if (!this.done)
            {
                this.OnMatchDoneAsync(match).Ignore();
            }
        }

        private async Task OnMatchDoneAsync(Carbon.Protocols.Match? match)
        {
            if (!this.done)
            {
                this.done = true;

                try
                {
                    if (match is null)
                    {
                        if (this.RoomType == MatchTypeConstant.PVP)
                        {
                            await this.SendMatchDoneAsync(-1, -1);
                        }
                    }
                    else
                    {
                        if (this.RoomType == MatchTypeConstant.PVP)
                        {
                            await this.SendMatchDoneAsync((int)match.Statistics.Home.Statistics.Runs, (int)match.Statistics.Away.Statistics.Runs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogUtility.Log(LogType.Error, "Room roomId={0} catched exception {1} when match-done.", this.RoomId, ex.Message);
                }
            }
        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="homeScore"></param>
        /// <param name="awayScore"></param>
        /// <returns></returns>
        private async Task SendMatchDoneAsync(int homeScore, int awayScore)
        {
            try
            {
                HttpClient httpClient = new();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                using StringContent jsonContent = new(JsonSerializer.Serialize(new ExternalMatchPVPEndRequest
                {
                    MatchType = this.RoomType,
                    RoomId = CtrlRoomId,
                    HomeId = this.Home,
                    AwayId = this.Away,
                    HomeScore = homeScore,
                    AwayScore = awayScore,
                }), Encoding.UTF8, "application/json");

                var ressponse = await httpClient.PostAsync(this.CallbackUrl, jsonContent);
                if (ressponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    LogUtility.Log(LogType.Error, "Room {0} requested received http error code {1}, content {2} on match-done .",
                        this.RoomId, ressponse.StatusCode, ressponse.Content);
                }
            }
            catch (Exception ex)
            {
                LogUtility.Log(LogType.Error, "Room {0} requested received exception {1} on match-done,handled {2} messages.", this.RoomId, ex.Message);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="pvpMessage"></param>
        public void SendMessage(long pid, PVPMessage pvpMessage)
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
