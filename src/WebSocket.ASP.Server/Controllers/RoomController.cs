using System;
using Microsoft.Extensions.DependencyInjection;
using Carbon.Match.New.Room;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Carbon.Match.Controllers
{
    public class RoomController : Controller
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<RoomController> logger;
        public RoomController(IServiceProvider serviceProvider, ILogger<RoomController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="roomType"></param>
        /// <param name="roomId"></param>
        /// <param name="home"></param>
        /// <param name="away"></param>
        /// <param name="callbackUrl"></param>
        /// <returns></returns>
        [Route("room/create")]
        public string Create(string roomType, long roomId, long home, long away, string callbackUrl)
        {
            var rmg = this.serviceProvider?.GetService<IRoomManager>();
            if (rmg != null)
            {
                this.logger.LogInformation($"Http request to create roomType={roomType} roomId ={roomId} home={home} away={away} callbackurl={callbackUrl}.");
                return rmg.CreateRoom(roomType, roomId, home, away, callbackUrl, this.logger);
            }
            return string.Empty;
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("api/ping")]
        public Task<Object> Ping(object postBody)
        {
            return Task.FromResult(postBody);
        }

        /// <summary>
        /// 检查房间
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public bool Exits(string roomId)
        {
            var rmg = this.serviceProvider?.GetService<IRoomManager>();
            if (rmg != null)
            {
                this.logger.LogInformation($"Http request to check  room {roomId} exists.");
                return rmg.GetRoom(roomId) != null;
            }
            return false;
        }
    }
}
