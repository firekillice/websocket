using System;
using Microsoft.Extensions.DependencyInjection;
using Carbon.Match.New.Room;
using Carbon.Match.New.Networking;
using Microsoft.AspNetCore.Mvc;

namespace Carbon.Match.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IServiceProvider serviceProvider;
        public DashboardController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

       [Route("Dashboard")]
        public object? Index()
        {
            var roomManager = this.serviceProvider.GetService<IRoomManager>();
            return roomManager != null ? (new { RoomCount = roomManager.Count(), Online = WebSocketHandler.Online() }) : (object)null;
        }
    }
}
