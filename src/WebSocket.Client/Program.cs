using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WSClient;

namespace Websocket.Client
{
    internal class Program
    {
        private static async Task Main()
        {
            await Task.WhenAll(Enumerable.Range(1, 1).Select(x => new ClientRoom(x).StartRoom()));

            Console.Read();
        }
    }
}
