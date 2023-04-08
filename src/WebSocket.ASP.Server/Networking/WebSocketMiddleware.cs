using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Carbon.Match.New.Networking
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate next;
        private WebSocketHandler webSocketHandler { get; set; }
        private readonly ILogger logger;
        private const int PageSize = 4 * 1024;

        public WebSocketMiddleware(RequestDelegate next, WebSocketHandler webSocketHandler, ILogger<WebSocketMiddleware> logger)
        {
            this.next = next;
            this.webSocketHandler = webSocketHandler;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await this.next(context);
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            this.webSocketHandler.OnConnected(context, socket);

            await this.ReceiveAsync(context, socket, async (messageType, buffer, count) =>
            {
                if (messageType == WebSocketMessageType.Binary)
                {
                    this.webSocketHandler.Receive(context, socket, buffer, count);
                    return;
                }
                else if (messageType == WebSocketMessageType.Close)
                {
                    this.webSocketHandler.OnDisconnected(context, socket);
                    return;
                }

            });
        }

        public async Task ReceiveAsync(HttpContext context, System.Net.WebSockets.WebSocket socket, Action<WebSocketMessageType, byte[], int> handleMessage)
        {
            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    var endOfMessage = false;
                    var writer = new ArrayBufferWriter<byte>();

                    while (!endOfMessage)
                    {
                        var buffer = writer.GetMemory(PageSize);
                        var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                        writer.Advance(result.Count);
                        endOfMessage = result.EndOfMessage;

                        if (result.EndOfMessage && (result.MessageType == WebSocketMessageType.Binary || result.MessageType == WebSocketMessageType.Close))
                        {
                            handleMessage(result.MessageType, writer.WrittenSpan.ToArray(), writer.WrittenCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, ex.Message, CancellationToken.None);
                    }
                    handleMessage(WebSocketMessageType.Close, null, 0);

                    this.logger.LogError($"Catched exception {ex.Message}");
                    break;
                }
            }
        }
    }
}
