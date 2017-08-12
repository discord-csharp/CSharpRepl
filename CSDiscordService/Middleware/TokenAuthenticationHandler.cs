using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Security.Claims;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;

namespace CSDiscordService.Middleware
{
    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Headers.TryGetValue("Authorization", out StringValues authorizationHeader))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authorization header missing"));
            }

            if (!AuthenticationHeaderValue.TryParse(authorizationHeader.ToString(), out var value))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authorization header malformed"));
            }

            if (value.Scheme != Options.AuthenticationType)
            {
                return Task.FromResult(AuthenticateResult.Fail("Authorization header malformed"));
            }

            if (Options.ValidTokens.Contains(value.Parameter))
            {
                Logger.LogInformation("Authenticated");
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(Options.AuthenticationType)), Options.AuthenticationType)));
            }

            return Task.FromResult(AuthenticateResult.Fail("invalid token"));
        }
    }
}
