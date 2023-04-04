using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Carbon.Match.New.Networking;
using System;
using Microsoft.Extensions.Logging;
using Carbon.Match.New.Room;

namespace Carbon.Match.New
{
    public class Program
    {
        private static void Main(string[] args)
        {
            new WebHostBuilder().ConfigureKestrel((context, options) =>
            {
                options.ListenAnyIP(80);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IRoomManager, RoomManager>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                //logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .UseKestrel().UseStartup<Startup>().Build().Run();
        }
    }
}

