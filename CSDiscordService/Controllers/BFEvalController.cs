using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using CSDiscordService.Eval;
using Microsoft.Extensions.Logging;
using CSDiscordService.Infrastructure;

namespace CSDiscordService.Controllers
{
    [Authorize(AuthenticationSchemes = "Token")]
    [Route("[controller]")]
    public class BFEvalController : Controller
    {
        private BrainfkEval _eval;
        private TelemetryClient _telemetryClient;
        private ILogger<EvalController> _logger;

        public BFEvalController(BrainfkEval eval, TelemetryClient telemetryClient, ILogger<EvalController> logger)
        {
            _eval = eval;
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        [HttpPost]
        [Produces("application/json")]
        [Consumes("text/plain")]
        public IActionResult Post([FromBody] string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            var result = _eval.RunEval(code);

            result.TrackResult(_telemetryClient, _logger);

            if (string.IsNullOrWhiteSpace(result.Exception))
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}
