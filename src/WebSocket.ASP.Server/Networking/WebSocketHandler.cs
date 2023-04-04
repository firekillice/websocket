using Carbon.Match.New.Contact;
using Carbon.Match.New.Room;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Protocols;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Carbon.Match.New.Networking
{
    public class WebSocketHandler
    {
        public const string WEBSOCKET_PLAYER_ID = "player-id";
        public const string WEBSOCKET_ROOM_ID = "room-id";

        private readonly ILogger<WebSocketHandler> logger;
        private readonly IServiceProvider serviceProvider;

        private static readonly ConcurrentDictionary<string, WebSocket> sockets = new();

        public WebSocketHandler(IServiceProvider serviceProvider, ILogger<WebSocketHandler> logger)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public static int Online() => sockets.Count;

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="websocket"></param>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        /// <exception cref="Exception"></exception>
        public void Receive(HttpContext context, WebSocket websocket, byte[] buffer, int count)
        {
            var pidString = context.Request.Headers[WEBSOCKET_PLAYER_ID];
            var roomId = context.Request.Headers[WEBSOCKET_ROOM_ID];
            if (string.IsNullOrEmpty(pidString) || string.IsNullOrEmpty(roomId))
            {
                throw new Exception(PVPExceptions.HEADER_VALUE_MISSING);
            }

            var rmg = this.serviceProvider.GetService<IRoomManager>();
            var room = rmg?.GetRoom(roomId);
            if (room == null)
            {
                throw new Exception(PVPExceptions.SPECIFIC_ROOM_NOT_EXISTS);
            }

            try
            {
                var pvpMessage = PvPMessage.Parser.ParseFrom(buffer, 0, count);
                if (pvpMessage != null)
                {
                    room.QueueMessage(long.Parse(pidString), context, pvpMessage);
                }
                else
                {
                    this.logger.LogWarning($"Player {pidString} receive empty message.");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Player {pidString} connection {context.Connection.Id} catched exception {ex.Message} when receive message.");
            }
 
        }

        public virtual void OnConnected(HttpContext context, WebSocket websocket)
        {
            var pidString = context.Request.Headers[WEBSOCKET_PLAYER_ID];
            var roomId = context.Request.Headers[WEBSOCKET_ROOM_ID];
            if (string.IsNullOrEmpty(pidString) || string.IsNullOrEmpty(roomId))
            {
                throw new Exception(PVPExceptions.HEADER_VALUE_MISSING);
            }

            var rmg = this.serviceProvider.GetService<IRoomManager>();
            var room = rmg?.GetRoom(roomId);
            if (room == null)
            {
                throw new Exception(PVPExceptions.SPECIFIC_ROOM_NOT_EXISTS);
            }

            sockets.TryAdd(context.Connection.Id, websocket);

            this.logger.LogInformation($"Player {pidString} from {context.Request.Host} connected, ready for roomId {roomId}, session {context.Connection.Id}.");

            try
            {
                var pid = long.Parse(pidString);
                if (!room.CheckPlayerExists(pid))
                {
                    throw new Exception(PVPExceptions.PLAYER_NOT_IN_SPECIFIC_ROOM);
                }
                var player = new RoomPlayer(pid, context, websocket);
                if (player != null)
                {
                    room.PlayerEnter(player);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Session received exception {ex.Message} when handing connected event");
            }
        }

        public void OnDisconnected(HttpContext context, WebSocket websocket)
        {
            if (context.Request.Headers.TryGetValue(WEBSOCKET_PLAYER_ID, out var pidString) && context.Request.Headers.TryGetValue(WEBSOCKET_ROOM_ID, out var roomId))
            {
                if (!string.IsNullOrEmpty(pidString) && !string.IsNullOrEmpty(roomId))
                {
                    var roomManager = this.serviceProvider.GetService<IRoomManager>();
                    if (roomManager != null)
                    {
                        var room = roomManager.GetRoom(roomId);
                        if (room != null)
                        {
                            room.PlayerLeave(long.Parse(pidString), context);
                        }
                        this.logger.LogInformation($"Player {pidString} in room {roomId} from {context.Request.Host} session {context.Connection.Id} is disconnected now.");
                    }
                }
                sockets.TryRemove(context.Connection.Id, out _);
            }
            else
            {
                this.logger.LogDebug($"Player disconcted session={2}, maybe http", context.Connection.Id);
            }
        }
    }
}
