using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Carbon.Match.Controllers
{
    public class MessageController : Controller
    {
        [HttpGet]
        public async Task SendMessage([FromQuery] string message)
        {
            await Task.FromResult(0);
        }

        [HttpGet]
        [Route("Message")]
        public async Task<string> IndexAsync([FromQuery] string message)
        {
            return await Task.FromResult("wwwwwwwwwwwww");
        }
    }
}
