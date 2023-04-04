using BeetleX.EventArgs;
using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.WebSockets;
using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Carbon.Match.Contact;
using Carbon.Match.Room;
using System;
using Carbon.Match.Utility;
using System.Collections.Concurrent;
using BeetleX;
using Server.Protocols;

namespace Carbon.Match.Networking
{
    public static class MessageHandler
    {
        public const string WEBSOCKET_PLAYER_ID = "player-id";
        public const string WEBSOCKET_ROOM_ID = "room-id";

        private static IServiceProvider? serviceProvider;
        private static ConcurrentDictionary<long, ISession> sessions = new();

        public static int Online() => sessions.Count;

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <exception cref="Exception"></exception>
        public static void OnWebsocketReceive(object? sender, WebSocketReceiveArgs args)
        {
            var pidString = args.Request.Header[WEBSOCKET_PLAYER_ID];
            var roomId = args.Request.Header[WEBSOCKET_ROOM_ID];
            if (string.IsNullOrEmpty(pidString))
            {
                throw new Exception(PVPExceptions.HEADER_VALUE_MISSING);
            }
            if (string.IsNullOrEmpty(roomId))
            {
                throw new Exception(PVPExceptions.HEADER_VALUE_MISSING);
            }

            var rmg = serviceProvider?.GetService<IRoomManager>();
            var room = rmg?.GetRoom(roomId);
            if (room == null)
            {
                throw new Exception(PVPExceptions.SPECIFIC_ROOM_NOT_EXISTS);
            }

            var pvpMessage = (PvPMessage)args.Frame.Body;
            if (pvpMessage != null)
            {
                //LogUtility.SessionLog(LogType.Info, args.Sesson, "Player {0} receive message type {1}.", pidString, pvpMessage.Type);
                room.QueueMessage(long.Parse(pidString), args.Sesson, pvpMessage);
            }
            else
            {
                LogUtility.SessionLog(LogType.Warring, args.Sesson, "Player {0} receive empty message.", pidString);
            }
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <exception cref="Exception"></exception>
        public static void OnWebsocketConnected(object? sender, WebSocketConnectArgs args)
        {
            var pidString = args.Request.Header[WEBSOCKET_PLAYER_ID];
            var roomId = args.Request.Header[WEBSOCKET_ROOM_ID];
            if (string.IsNullOrEmpty(pidString))
            {
                throw new Exception(PVPExceptions.HEADER_VALUE_MISSING);
            }
            if (string.IsNullOrEmpty(roomId))
            {
                throw new Exception(PVPExceptions.HEADER_VALUE_MISSING);
            }

            if (serviceProvider == null)
            {
                serviceProvider = args.Request.Server.ServiceProvider();
            }

            var rmg = serviceProvider.GetService<IRoomManager>();
            var room = rmg?.GetRoom(roomId);
            if (room == null)
            {
                throw new Exception(PVPExceptions.SPECIFIC_ROOM_NOT_EXISTS);
            }

            sessions.TryAdd(args.Request.Session.ID, args.Request.Session);

            LogUtility.SessionLog(LogType.Info, args.Request.Session,
                "Player {0} from {1} connected, ready for roomId {2}, session {3}.",
                pidString, $"{args.Request.Session.Host}:{args.Request.Session.Port}", roomId, args.Request.Session.ID);

            try
            {
                var pid = long.Parse(pidString);
                if (!room.CheckPlayerExists(pid))
                {
                    throw new Exception(PVPExceptions.PLAYER_NOT_IN_SPECIFIC_ROOM);
                }
                var player = new RoomPlayer(pid, args.Request.Session, args.Request.Server);
                if (player != null)
                {
                    args.Request.Session[WEBSOCKET_PLAYER_ID] = pidString;
                    args.Request.Session[WEBSOCKET_ROOM_ID] = roomId;
                    room.PlayerEnter(player);
                }
            }
            catch (Exception ex)
            {
                LogUtility.SessionLog(LogType.Error, args.Request.Session,
                "Session received exception {0} when handing connected event", ex.Message);
            }
        }

        /// <summary>
        /// 连接断开
        /// HTTP的断开也会进来
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <exception cref="Exception"></exception>
        public static void OnWebsocketDisconnected(object? sender, SessionEventArgs args)
        {
            var pidString = (string)args.Session[WEBSOCKET_PLAYER_ID];
            var roomId = (string)args.Session[WEBSOCKET_ROOM_ID];
            if (!string.IsNullOrEmpty(pidString) && !string.IsNullOrEmpty(roomId))
            {
                var roomManager = serviceProvider?.GetService<IRoomManager>();
                if (roomManager != null)
                {
                    var room = roomManager.GetRoom(roomId);
                    if (room != null)
                    {
                        room.PlayerLeave(long.Parse(pidString), args.Session);
                    }
                    LogUtility.SessionLog(LogType.Info, args.Session, "Player {0} in room {1} from {2} session {3} is disconnected now.",
                        pidString, roomId, $"{args.Session.Host}:{args.Session.Port}", args.Session.ID);
                }
            }
            else
            {
                LogUtility.SessionLog(LogType.Debug, args.Session, "Player disconcted pid={0} room={1} session={2}, maybe http", pidString, roomId, args.Session.ID);
            }
            sessions.TryRemove(args.Session.ID, out _);
        }
    }
}
