using System;
using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Carbon.Match.Room;
using BeetleX.EventArgs;
using Carbon.Match.Utility;

namespace Carbon.Match.Controller
{
    [BeetleX.FastHttpApi.Controller(BaseUrl = "/room")]
    public class RoomController : BeetleX.FastHttpApi.IController
    {
        private IServiceProvider? serviceProvider;

        [BeetleX.FastHttpApi.NotAction]
        public void Init(HttpApiServer server, string path)
        {
            this.serviceProvider = server.ServiceProvider();
        }

        /// <summary>
        /// callbackUrl
        /// </summary>
        /// <param name="roomType"></param>
        /// <param name="roomId"></param>
        /// <param name="home"></param>
        /// <param name="away"></param>
        /// <param name="callbackUrl"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string Create(string roomType, long roomId, long home, long away, string callbackUrl, IHttpContext context)
        {
            var rmg = this.serviceProvider?.GetService<IRoomManager>();
            if (rmg != null)
        {
                LogUtility.SessionLog(LogType.Info, context.Session,
                    "Http request to create roomType={0} roomId ={1} home={2} away={3} callbackurl={4}.",
                    roomType, roomId, home, away, callbackUrl);
                return rmg.CreateRoom(roomType, roomId, home, away, callbackUrl);
            }
            return string.Empty;
        }

        /// <summary>
        /// 检查房间
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool Exits(string roomId, IHttpContext context)
        {
            var rmg = this.serviceProvider?.GetService<IRoomManager>();
            if (rmg != null)
            {
                LogUtility.SessionLog(LogType.Debug, context.Session, "Http request to check  room {0} exists.", roomId);
                return rmg.GetRoom(roomId) != null;
            }
            return false;
        }
    }
}
