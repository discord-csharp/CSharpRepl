using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CSDiscordService.Controllers
{ 
    [Authorize(AuthenticationSchemes = "Token")]
    [Route("[controller]")]
    public class EvalController : Controller
    {
        private Eval _eval;
        private TelemetryClient _telemetryClient;

        public EvalController(Eval eval, TelemetryClient telemetryClient)
        {
            _eval = eval;
            _telemetryClient = telemetryClient;
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
