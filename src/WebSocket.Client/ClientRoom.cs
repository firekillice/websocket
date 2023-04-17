using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WebSocket.Client;
using WebSocket.Client.Replay;

namespace WSClient
{
    public class ClientRoom : IClientRoom
    {
        private readonly long clientRoomId;
        private readonly long home;
        private readonly long away;
        private string? pVPRoomId;
        private readonly WebSocketHost wsHostInfo;

        public ClientRoom(long roomId)
        {
            this.clientRoomId = roomId * 3;
            this.home = roomId * 3 - 2;
            this.away = roomId * 3 - 1;
            this.wsHostInfo = ClientConfig.GetHost(roomId);
            Console.WriteLine($"room id {roomId}");
        }

        public async Task StartRoom(ReplayReader.BiInputs biInputs)
        {
            var homeEmulator = default(ClientEmulator);
            var awayEmulator = default(ClientEmulator);
            while (true)
            {
                try
                {
                    this.pVPRoomId = await CreateRoomAsync("PvP", this.clientRoomId, this.home, this.away);
                    if (string.IsNullOrEmpty(this.pVPRoomId))
                    {
                        await Task.Delay(new Random().Next(2000, 5000));
                        continue;
                    }
                    homeEmulator = new ClientEmulator(this.home, this.pVPRoomId, this.wsHostInfo.WebSocket, biInputs.HomeInputs);
                    awayEmulator = new ClientEmulator(this.away, this.pVPRoomId, this.wsHostInfo.WebSocket, biInputs.AwayInputs);
                    await Task.WhenAny(homeEmulator.RunAsync(), awayEmulator.RunAsync());

                    try
                    {
                        await homeEmulator.DestoryAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"homeEmulator catch {ex.Message}");
                    }

                    try
                    {
                        await awayEmulator.DestoryAsync();
                    }
                    catch (Exception ex)
                    { 
                        Console.WriteLine($"awayEmulator catch {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"StartRoom cactch {ex.Message}");
                }
                finally
                {
                    await Task.Delay(new Random().Next(2000, 5000));
                }
            }
        }

        private async Task<string> CreateRoomAsync(string roomType, long roomId, long home, long away)
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
                var response = await httpClient.PostAsJsonAsync($"{this.wsHostInfo.Http}sys/createroom", request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var roomInfo = (await response.Content.ReadFromJsonAsync<CreateRoomResponseDto>());
                    return roomInfo.RoomId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateRoomAsync{ex.Message}");
            }
            return null;
        }
    }
}
