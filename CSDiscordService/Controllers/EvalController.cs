using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using CSDiscordService.Eval;
using CSDiscordService.Eval.ResultModels;
using Microsoft.Extensions.Logging;

namespace CSDiscordService.Controllers
{
    [Authorize(AuthenticationSchemes = "Token")]
    [Route("[controller]")]
    public class EvalController : Controller
    {
        private CSharpEval _eval;
        private TelemetryClient _telemetryClient;
        private ILogger<EvalController> _logger;

        public EvalController(CSharpEval eval, TelemetryClient telemetryClient, ILogger<EvalController> logger)
        {
            _eval = eval;
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        [HttpPost]
        [Produces("application/json")]
        [Consumes("text/plain")]
        public async Task<IActionResult> Post([FromBody] string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            var result = await _eval.RunEvalAsync(code);

            TrackResult(result);

            if (string.IsNullOrWhiteSpace(result.Exception))
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        private void TrackResult(EvalResult result)
        {
            try
            {
                var evt = new EventTelemetry("eval")
                {
                    Timestamp = DateTimeOffset.UtcNow
                };

                evt.Metrics.Add("CompileTime", result.CompileTime.TotalMilliseconds);
                evt.Metrics.Add("ExecutionTime", result.ExecutionTime.TotalMilliseconds);

                evt.Properties.Add("Code", result.Code);
                evt.Properties.Add("ConsoleOut", result.ConsoleOut);
                evt.Properties.Add("ReturnValue", JsonConvert.SerializeObject(result.ReturnValue, Formatting.Indented));
                evt.Properties.Add("ExceptionType", result.ExceptionType);
                evt.Properties.Add("Exception", result.Exception);

                _telemetryClient.TrackEvent(evt);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to record telemetry event: {ex}");
            }
        }
    }
}
