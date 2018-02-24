using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CSDiscordService.Eval;

namespace CSDiscordService.Controllers
{
    [Authorize(AuthenticationSchemes = "Token")]
    [Route("[controller]")]
    public class ILController : Controller
    {
        private TelemetryClient _telemetryClient;
        private ILogger<ILController> _logger;
        private DisassemblyService _dasmService;

        public ILController(DisassemblyService dasmService, TelemetryClient telemetryClient, ILogger<ILController> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
            _dasmService = dasmService;
        }

        [HttpPost]
        [Produces("text/plain")]
        [Consumes("text/plain")]
        public async Task<IActionResult> Post([FromBody] string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }
            var final = _dasmService.GetIl(code);

            _logger.LogInformation(final);

            return Ok(final);
        }
    }
}

