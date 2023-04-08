using System.Collections.Generic;

namespace WebSocket.Client
{
    public record WebSocketHost(string Http, string WebSocket);

    public static class ClientConfig
    {
        //public  readonly static List<WebSocketHost> Hosts = new() {
        //    new("http://10.10.50.163:8975/","ws://10.10.50.163:8975/ws"), 
        //};
        public readonly static List<WebSocketHost> Hosts = new() {
            new("http://192.168.30.27:8975/","ws://192.168.30.27:8975/ws"),
            new("http://192.168.30.26:8975/","ws://192.168.30.26:8975/ws")
        };
        public static WebSocketHost GetHost(long index)
        {
            return Hosts[(int)(index % Hosts.Count)];
        }
    }
}
