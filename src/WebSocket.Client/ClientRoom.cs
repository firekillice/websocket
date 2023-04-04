using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

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
            this.pVPRoomId = await CreateRoomAsync("PvP", this.clientRoomId, this.home, this.away);
            if (string.IsNullOrEmpty(this.pVPRoomId))
            {
                throw new ArgumentException("create_room_error");
            }
            Console.WriteLine($"CreateRoom Success RoomId {this.pVPRoomId}");

            await Task.WhenAll(new PBClient(this.home, this.pVPRoomId).RunAsync(), new PBClient(this.away, this.pVPRoomId).RunAsync());
        }

        private static async Task<string> CreateRoomAsync(string roomType, long roomId, long home, long away)
        {
            try
            {
                var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };
                httpClient.DefaultRequestHeaders.Add("pvp-token", "fRSAGAqBZ6k72FnlwR9uoKLS");

                var request = new CreateRoomRequestDto
                {
                    AwayId = away,
                    HomeId = home,
                    RoomId = roomId,
                    Type = roomType,
                };
                var response = await httpClient.PostAsJsonAsync($"http://localhost:8975/sys/createroom", request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var roomInfo = (await response.Content.ReadFromJsonAsync<CreateRoomResponseDto>());
                    return roomInfo.RoomId;
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
    }
}
