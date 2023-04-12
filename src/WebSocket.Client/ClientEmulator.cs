using Carbon.PvP.Replay;
using Google.Protobuf;
using Server.Protocols;
using System;
using System.Collections.Immutable;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static WebSocket.Client.Replay.ReplayReader;

namespace WSClient
{
    public class MessageComplexHandler
    {
        private int index = 0;
        private readonly ImmutableArray<Record> records;
        private int sequence = 0;
        private object locker = new object();
        private System.Timers.Timer pingTimer;
       

        public MessageComplexHandler(ImmutableArray<Record> inputs)
        {
            this.records = inputs;
        }

        private void SetPingTimer(ClientWebSocket socket)
        {
            this.pingTimer = new System.Timers.Timer(2000);
            this.pingTimer.Elapsed += async (s, e) =>
            {
                await sendMessageAsync(socket, null, true);
            };
            this.pingTimer.AutoReset = true;
            this.pingTimer.Start();
        }

        public void Destroy()
        {
            if (this.pingTimer != default)
            {
                this.pingTimer.Stop();
            }
        }

        private void AddTimer(long interval, Func<Task> handler, ClientWebSocket socket)
        {
            var timer = new System.Timers.Timer();
            timer.Interval = interval;
            timer.Elapsed += (s, e) => handler();
            timer.AutoReset = false;
            timer.Start();
        }

        public void StarSendTimer(ClientWebSocket socket)
        {
            async Task handleRecord()
            {
                await this.sendMessageAsync(socket, this.records[this.index], false);
                this.index++;
                if (this.index == records.Length - 1)
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure, "normal", CancellationToken.None);
                        Console.WriteLine($"abort socket, index {this.index}");
                }

                if (this.index <= this.records.Length - 2)
                {
                    while (this.index <= this.records.Length - 2 && this.records[this.index + 1].TimestampMs <= this.records[this.index].TimestampMs)
                    {
                        await this.sendMessageAsync(socket, this.records[this.index], false);
                        this.index++;
                    }
                    AddTimer(this.records[this.index + 1].TimestampMs - this.records[this.index].TimestampMs, handleRecord, socket);
                }
            }

            this.AddTimer(new Random().Next(1, 500), handleRecord, socket);
            this.SetPingTimer(socket);
        }

        private async Task sendMessageAsync(ClientWebSocket socket, Record r, bool isPing)
        {
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    if (!isPing)
                    {
                        var sendBytes = default(byte[]);
                        lock (this.locker)
                        {
                            sendBytes = MessageConverter.Write(r.Message.ToByteArray(), (uint)Interlocked.Increment(ref this.sequence), 0).ToByteArray();
                        }

                        var bytesToSend = new ArraySegment<byte>(sendBytes);
                        await socket.SendAsync(bytesToSend, WebSocketMessageType.Binary, true, CancellationToken.None);

                        //Console.WriteLine($"send message bytes {bytesToSend.Count} , index {this.index}");
                    }
                    else
                    {
                        var pvpMessage = default(PvPMessage);
                        lock (this.locker)
                        {
                            pvpMessage = new PvPMessage
                            {
                                Type = -20,
                                Flags = 50,
                                Seq = (uint)Interlocked.Increment(ref this.sequence),
                                Sseq = 0
                            };
                        }

                        var bytesToSend = new ArraySegment<byte>(pvpMessage.ToByteArray());
                        await socket.SendAsync(bytesToSend, WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"socket exception {ex.Message} ");
            }

        }
    }

    public class ClientEmulator : IClientEmulator
    {
        private readonly long pid;
        private readonly string roomId;
        private readonly string wshost;
        private MessageComplexHandler handler;
        private ClientWebSocket websocket;
        private bool canceled = false;

        public ClientEmulator(long pid, string roomId, string wshost, ImmutableArray<Record> records)
        {
            this.pid = pid;
            this.roomId = roomId;
            this.wshost = wshost;
            this.handler = new MessageComplexHandler(records);
        }

        public  async Task DestoryAsync()
        {
            if (this.websocket.State == WebSocketState.Open)
            {
                await this.websocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure, "normal", CancellationToken.None);
                Console.WriteLine("dispose emulator");
            }
            this.canceled = true;
            this.handler.Destroy();
        }
        public async Task RunAsync()
        {
            try
            {
                this.websocket = new ClientWebSocket();
                var serverUri = new Uri(this.wshost);
                this.websocket.Options.SetRequestHeader("player-id", pid.ToString());
                this.websocket.Options.SetRequestHeader("room-id", roomId);
                while (!this.canceled && this.websocket.State != WebSocketState.Open)
                {
                    try
                    {
                        await this.websocket.ConnectAsync(serverUri, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"catch exception {ex.Message} {this.wshost} {this.pid}");
                        await Task.Delay(5000);
                    }
                }

                if (this.websocket.State == WebSocketState.Open)
                {
                    this.handler.StarSendTimer(this.websocket);

                    await Task.Run(async () => await RecvAsync(this.websocket));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"catch exception {ex.Message} ");
            }
        }

        public async Task RecvAsync(ClientWebSocket socket)
        {
            while (socket.State == WebSocketState.Open)
            {
                var buffer = new byte[16384 * 3];
                try
                {
                    var response = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var rcvpvpmessage = PvPMessage.Parser.ParseFrom(buffer, 0, response.Count);
                    // Console.WriteLine($"{this.pid} receving message");
                }
                catch (OperationCanceledException)
                {
                    continue;
                }

                await Task.Delay(5);
            }
        }
    }
}
