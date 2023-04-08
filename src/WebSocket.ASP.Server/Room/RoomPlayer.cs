using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Carbon.Match.New.Contact;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Server.Protocols;

namespace Carbon.Match.New.Room
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
        /// http上下文
        /// </summary>
        public HttpContext Context { get;}
        /// <summary>
        /// 绑定的连接
        /// </summary>
        public System.Net.WebSockets.WebSocket Websocket { get; set; } = default;

        public RoomPlayer(long pid, HttpContext context, System.Net.WebSockets.WebSocket websocket)
        {
            this.Pid = pid;
            this.Context = context;
            this.Websocket = websocket;
        }

        /// <summary>
        /// 发送内容
        /// </summary>
        /// <param name="message"></param>
        public async Task SendAsync(PvPMessage message, ILogger logger)
        {
            try
            {
                if (this.Context != null)
                {
                    if (this.Websocket.State == WebSocketState.Open)
                    {
                        var bytes = message.ToByteArray();

                        await this.Websocket.SendAsync(buffer: new ArraySegment<byte>(bytes, 0, bytes.Length),
                                    messageType: WebSocketMessageType.Binary,
                                    endOfMessage: true,
                                    cancellationToken: CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Player {this.Pid} received exception {ex.Message} when sending message {message.Type} {message.Seq} {message.Sseq}" );
            }

        }
    }
}
