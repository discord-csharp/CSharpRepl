using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using CSDiscordService.Eval;
using Microsoft.Extensions.Logging;
using CSDiscordService.Eval.ResultModels;

namespace CSDiscordService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EvalController : ControllerBase
    {
        private readonly CSharpEval _eval;
        private readonly ILogger<EvalController> _logger;

        public EvalController(CSharpEval eval, ILogger<EvalController> logger)
        {
            _eval = eval;
            _logger = logger;
        }

        [HttpPost]
        [Consumes("text/plain")]
        public async Task<ActionResult<EvalResult>> Post([FromBody] string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            using (_logger.BeginScope(this))
            {
                _logger.LogInformation("{code}", code);

                var result = await _eval.RunEvalAsync(code);

                if(Request.Headers.ContainsKey("X-Modix-DiscordUserId"))
                {
                    _logger.LogInformation("Headers: {X-Modix-DiscordUserId}\n{X-Modix-DiscordUsername}\n{X-Modix-MessageLink}", 
                        Request.Headers["X-Modix-DiscordUserId"][0],
                        Request.Headers["X-Modix-DiscordUsername"][0],
                        Request.Headers["X-Modix-MessageLink"][0]);
                }

                _logger.LogInformation("{status}: {result}", string.IsNullOrWhiteSpace(result.Exception) ? "Successful" : "Failed", result);

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
}
