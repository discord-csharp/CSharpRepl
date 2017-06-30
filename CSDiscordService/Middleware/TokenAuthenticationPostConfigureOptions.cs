
using Microsoft.Extensions.Options;

namespace CSDiscordService.Middleware
{
    public class TokenAuthenticationPostConfigureOptions : IPostConfigureOptions<TokenAuthenticationOptions>
    {
        public void PostConfigure(string name, TokenAuthenticationOptions options)
        {
            
        }
    }
}
