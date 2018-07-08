using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CSDiscordService.Eval;

namespace CSDiscordService.Controllers
{
    [Route("[controller]")]
    public class ILController : ControllerBase
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<ILController> _logger;
        private readonly DisassemblyService _dasmService;

        public ILController(DisassemblyService dasmService, TelemetryClient telemetryClient, ILogger<ILController> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
            _dasmService = dasmService;
        }

        [HttpPost]
        [Produces("text/plain")]
        [Consumes("text/plain")]
        public Task<ActionResult<string>> Post([FromBody] string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }
            var final = _dasmService.GetIl(code);

            _logger.LogInformation(final);

            return Task.FromResult<ActionResult<string>>(Ok(final));
        }
    }
}

