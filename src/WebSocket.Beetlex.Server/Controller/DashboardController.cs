using System;
using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Carbon.Match.Room;
using Carbon.Match.Networking;

namespace Carbon.Match.Controller
{
    [BeetleX.FastHttpApi.Controller(BaseUrl = "/dashboard")]
    public class DashboardController : BeetleX.FastHttpApi.IController
    {
        private IServiceProvider? serviceProvider;

        [BeetleX.FastHttpApi.NotAction]
        public void Init(HttpApiServer server, string path) 
        {
            this.serviceProvider = server.ServiceProvider();
        }

        /// <summary>
        /// 主要情况
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object? Main(IHttpContext context)
        {
            var rmg = this.serviceProvider?.GetService<IRoomManager>();
            if (rmg != null)
            {
                return new { RoomCount = rmg.Count(), Online = MessageHandler.Online() };
            }
            return null;
        }   
    }
}
