using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CSDiscordService.Controllers
{
    [Route("[controller]")]
    public class StatusController : Controller
    {
        private ILogger<StatusController> _logger;

        public StatusController(ILogger<StatusController> logger)
        {
            _logger = logger;
        }

        [HttpGet("probe")]
        public async Task<ActionResult<string>> StatusProbeAsync()
        {
            await Task.CompletedTask;
            return Ok("Ok");
        }
    }
}
