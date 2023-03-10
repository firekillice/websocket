using Carbon.Match.Contact;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Carbon.Match.Room
{
    /// <summary>
    /// 房间管理器
    /// </summary>
    public class RoomManager : IRoomManager
    {
        private readonly object lockable = new();

        private static readonly Dictionary<string, EmulatorRoom> Rooms = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 添加房间
        /// </summary>
        /// <param name="roomType"></param>
        /// <param name="roomId"></param>
        /// <param name="home"></param>
        /// <param name="away"></param>
        /// <param name="callbackUrl"></param>
        /// <returns></returns>
        public string CreateRoom(string roomType, long roomId, long home, long away, string callbackUrl)
        {
            var room = new EmulatorRoom(this, roomType, roomId, home, away, callbackUrl);
            lock (this.lockable)
            {
                Rooms[room.RoomId] = room;
            }
            return room.RoomId;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        /// <param name="roomId"></param>
        public void DestoryRoom(string roomId)
        {
            lock (this.lockable)
            {
                if (Rooms.Remove(roomId, out var room))
                {
                    room.Dispose();
                }
            }
        }

        public void Dispose() => Rooms.Clear();

        public IEmulatorRoom? GetRoom(string roomId)
        {
            lock (this.lockable)
            {
                return Rooms.TryGetValue(roomId, out var room) ? room : null;
            }
        }

        public int Count() => Rooms.Count;
    }
}
