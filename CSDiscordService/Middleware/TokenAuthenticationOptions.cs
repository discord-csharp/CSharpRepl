
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;

namespace CSDiscordService.Middleware
{
    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string AuthenticationType { get; set; } = TokenAuthenticationExtensions.AuthenticationTypeDefault;
        public List<string> ValidTokens { get; set; } = new List<string>();
    }
}
