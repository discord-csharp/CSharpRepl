using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CSDiscordService.Controllers
{
    [Route("[controller]")]
    public class StatusController : Controller
    {
        public StatusController()
        {
        }

        [HttpGet("probe")]
        public async Task<ActionResult<string>> StatusProbeAsync()
        {
            await Task.CompletedTask;
            return Ok("Ok");
        }
    }
}
