using Google.Protobuf;
using Server.Protocols;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WSClient
{
    public class PBClient : IClient
    {
        private readonly long pid;
        private readonly string roomId;

        public PBClient(long pid, string roomId)
        {
            this.pid = pid;
            this.roomId = roomId;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                await this.OneCircleAsync();
            }
        }

        public async Task OneCircleAsync()
        {
            try
            {
                var webSocketUrl = $"ws://localhost:8975/ws";
                var clientWebSocket = new ClientWebSocket();
                var serverUri = new Uri(webSocketUrl);
                clientWebSocket.Options.SetRequestHeader("player-id", pid.ToString());
                clientWebSocket.Options.SetRequestHeader("room-id", roomId);
                await clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);

                if (clientWebSocket.State != WebSocketState.Open)
                {
                    await clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
                }

                uint seq = 0;
                while (true)
                {
                    if (clientWebSocket.State == WebSocketState.Open)
                    {
                        var pvpMessage = new PvPMessage
                        {
                            Type = 1,
                            Flags = 50,
                            Seq = ++seq,
                            Sseq = seq + 1,
                            //Raw = ByteString.CopyFrom(new byte[] { 1, 2, 3 })
                        };
                        var lenth = new Random().Next(1, 16384);
                        var rawBytes = new byte[lenth];
                        for (var i = 0; i < lenth; i++)
                        {
                            rawBytes[i] = (byte)new Random().Next(1, 254);
                        }
                        pvpMessage.Body = ByteString.CopyFrom(rawBytes);

                       // Console.WriteLine($"{ this.pid} sending message");

                        var sendBytes = pvpMessage.ToByteArray();
                        var bytesToSend = new ArraySegment<byte>(sendBytes);
                        await clientWebSocket.SendAsync(bytesToSend, WebSocketMessageType.Binary, true, CancellationToken.None);

                        var buffer = new byte[16384 * 3];
                        var timeOut = new CancellationTokenSource(2000).Token;
                        var response = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), timeOut);

                        var rcvpvpmessage = PvPMessage.Parser.ParseFrom(buffer, 0, response.Count);
                       // Console.WriteLine($"{ this.pid } receving message");

                        var randomResult = new Random().Next(100);
                        if (randomResult < 20)
                        {
                            clientWebSocket.Abort();
                            Console.WriteLine("random close+break");
                            break;
                        }
                        else if (randomResult < 40)
                        {
                            Console.WriteLine("random break");
                            break;
                        }
                        else if (randomResult < 60)
                        {
                            await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, default);
                            Console.WriteLine("random shutdown");
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine(clientWebSocket.State);
                    }

                    await Task.Delay(5);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
        }

    
    }


}
