using System;
using System.Collections.Generic;
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
            List<Task> tasks = new List<Task>();
            var inputs = LoadReplayRecord();
            var step = 100;
            for (var i = 1; i <= 2000; i += step)
            {
                Console.WriteLine($"start index {i}");
                var index = i;
                tasks.AddRange(Enumerable.Range(index, step).Select(x => new ClientRoom(x).StartRoom(inputs)));
              
                Task.Delay(1000).Wait();
            }
            await Task.WhenAll(tasks);

            Console.Read();
        }

        private static ReplayReader.BiInputs LoadReplayRecord()
        {
            return ReplayReader.Read(@"./Config/rec-c86cf504-9c08-47ee-9cd7-38a613be26a9-1.json",
                @"./Config/rec-c86cf504-9c08-47ee-9cd7-38a613be26a9-2.json");
        }
    }
}
