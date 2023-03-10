using WSClient;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Threading;

namespace Websocket.Client
{
    internal class Program
    {
        private static async Task Main()
        {
            ThreadPool.SetMinThreads(100, 100);


            await Task.WhenAll(Enumerable.Range(1, 100).Select(x => new ClientRoom(x).StartRoom()));

            Console.Read();
        }
    }
}
