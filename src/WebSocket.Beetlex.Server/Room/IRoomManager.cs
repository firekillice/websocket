using System;
using System.Collections.Generic;
using Carbon.Match.Contact;

namespace Carbon.Match.Room
{
    public interface IRoomManager : IDisposable
    {
        public string CreateRoom(string roomType, long roomId, long home, long away, string callbackUrl);

        public void DestoryRoom(string roomId);

        public IEmulatorRoom? GetRoom(string roomId);

        public int Count();
    }
}
