using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CSDiscordService.Controllers
{

    [Route("[controller]")]
    public class EvalController : Controller
    {
        private Eval _eval;

        public EvalController(Eval eval)
        {
            _eval = eval;
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
