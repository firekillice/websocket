using System;
using BeetleX;
using BeetleX.EventArgs;
using BeetleX.FastHttpApi.WebSockets;
using Carbon.Match.Contact;
using Carbon.Match.Utility;
using Server.Protocols;

namespace Carbon.Match.Room
{
    /// <summary>
    /// 玩家基础信息
    /// </summary>
    public class RoomPlayer
    {
        /// <summary>
        /// 玩家角色Id
        /// </summary>
        public long Pid { get; set; } = 0;
        /// <summary>
        /// 绑定的连接
        /// </summary>
        public ISession? Session { get; set; } = default;
        /// <summary>
        /// 绑定的HTTPServer
        /// </summary>
        public IWebSocketServer? Server { get; set; } = default;

        public RoomPlayer(long pid, ISession session, IWebSocketServer server)
        {
            this.Pid = pid;
            this.Session = session;
            this.Server = server;
        }

        /// <summary>
        /// 发送内容
        /// </summary>
        /// <param name="message"></param>
        public void Send(PvPMessage message)
        {
            try
            {
                if (this.Server != null && this.Session != null)
                {
                    if (!this.Session.IsDisposed)
                    {
                        var frame = this.Server.CreateDataFrame(message);
                        frame.Type = DataPacketType.binary;
                        frame.Send(this.Session);
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtility.SessionLog(LogType.Warring, this.Session, "Player {0} received exception {1} when sending message type={2} seq={3} sseq={4}",
                    this.Pid, ex.Message, message.Type, message.Seq, message.Sseq);
            }

        }
    }
}
