using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Carbon.Match.FrameSerializer;
using Carbon.Match.Networking;
using Carbon.Match.Room;
using Carbon.Match.Utility;

public class Program
{
    private static void Main()
    {
        var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IRoomManager, RoomManager>();

                    services.UseBeetlexHttp(o =>
                    {
                        o.LogToConsole = true;
                        o.ManageApiEnabled = false;
                        o.Port = 80;
                        o.SetDebug();
                        o.LogLevel = BeetleX.EventArgs.LogType.Warring;
                        o.IOQueueEnabled = true;
                    },

                    http =>
                    {
                        http.WebSocketReceive = (o, e) =>
                        {
                            MessageHandler.OnWebsocketReceive(o, e);
                        };
                        http.WebSocketConnect += (o, e) =>
                        {
                            MessageHandler.OnWebsocketConnected(o, e);
                        };
                        http.HttpDisconnect += (o, e) =>
                        {
                            MessageHandler.OnWebsocketDisconnected(o, e);
                        };
                        http.Started += (o, e) =>
                        {
                            LogUtility.Server = http.BaseServer;
                        };
                        http.FrameSerializer = new PVPFrameSerializer();
                    },
                     typeof(Program).Assembly);
                });
        builder.Build().Run();
    }
}
