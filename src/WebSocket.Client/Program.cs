using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSocket.Client.Replay;
using WSClient;
using static WebSocket.Client.Replay.ReplayReader;

namespace Websocket.Client
{
    internal class Program
    {
        private static async Task Main()
        {
            var inputs = LoadReplayRecord();
            await Task.WhenAll(Enumerable.Range(1, 1).Select(x => new ClientRoom(x).StartRoom(inputs)));

            Console.Read();
        }

        private static ReplayReader.BiInputs LoadReplayRecord()
        {
            return ReplayReader.Read(@"./Config/rec-c86cf504-9c08-47ee-9cd7-38a613be26a9-1.json",
                @"./Config/rec-c86cf504-9c08-47ee-9cd7-38a613be26a9-2.json");
        }
    }
}
