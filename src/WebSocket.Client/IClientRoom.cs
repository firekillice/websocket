using System.Threading.Tasks;
using WebSocket.Client.Replay;

namespace WSClient
{
    public interface IClientRoom
    {
        public Task StartRoom(ReplayReader.BiInputs biInputs);
    }
}
