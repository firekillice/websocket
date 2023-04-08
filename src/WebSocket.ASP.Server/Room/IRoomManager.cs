using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Carbon.Match.New.Room
{
    public interface IRoomManager : IDisposable
    {
        public string CreateRoom(string roomType, long roomId, long home, long away, string callbackUrl, ILogger logger);

        public void DestoryRoom(string roomId);

        public IEmulatorRoom? GetRoom(string roomId);

        public int Count();
    }
}
