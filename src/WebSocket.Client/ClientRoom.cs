using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using Orleans;

namespace WSClient
{
    public class ClientRoom : IClientRoom
    {
        private readonly long clientRoomId;
        private readonly long home;
        private readonly long away;
        private string? pVPRoomId;

        public ClientRoom(long roomId)
        {
            if (roomId > 10 * 10000)
            {
                throw new ArgumentException("room_id_exceed");
            }

            this.clientRoomId = roomId;
            this.home = roomId * 1000;
            this.away = roomId * 1000 + 1;
        }

        public async Task StartRoom()
        {
            //for (var i = 0; i < 10000; i++)
            //{
            //    var cbUrl = HttpUtility.UrlEncode("http://127.0.0.1:8975/api/ExternalMatch/PVPMatchDone");
            //    this.pVPRoomId = await CreateRoomAsync("PvP", this.clientRoomId, this.home, this.away, cbUrl);
            //    if (string.IsNullOrEmpty(this.pVPRoomId))
            //    {
            //        throw new ArgumentException("create_room_error");
            //    }
            //    Console.WriteLine($"CreateRoom Success RoomId {this.pVPRoomId}");
            //}
            var cbUrl = HttpUtility.UrlEncode("http://127.0.0.1:8975/api/ExternalMatch/PVPMatchDone");
            this.pVPRoomId = await CreateRoomAsync("PvP", this.clientRoomId, this.home, this.away, cbUrl);
            if (string.IsNullOrEmpty(this.pVPRoomId))
            {
                throw new ArgumentException("create_room_error");
            }
            Console.WriteLine($"CreateRoom Success RoomId {this.pVPRoomId}");

            await Task.WhenAll(new PBClient(this.home, this.pVPRoomId).RunAsync(), new PBClient(this.away, this.pVPRoomId).RunAsync());
        }

        private static async Task<string> CreateRoomAsync(string roomType, long roomId, long home, long away, string cbUrl)
        {
            var client = new HttpClient();
            try
            {
                var response = await client.GetAsync($"http://localhost/room/create?roomType={roomType}&&roomId={roomId}&&home={home}&&away={away}&&callbackUrl={cbUrl}");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine(response);

                    var data = await new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEndAsync();
                    var jo = JObject.Parse(data);
                    if (jo.ContainsKey("Data"))
                    {
                        return jo["Data"]?.ToString() ?? string.Empty;
                    }
                }
                else
                {
                    Console.WriteLine($"Create room status {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return string.Empty;
        }
    }
}
