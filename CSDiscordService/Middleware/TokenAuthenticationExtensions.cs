using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace CSDiscordService.Middleware
{
    public static class TokenAuthenticationExtensions
    {
        public static string AuthenticationTypeDefault = "Token";

        public static IServiceCollection AddTokenAuthentication(this IServiceCollection services, string authenticationScheme) =>
            AddTokenAuthentication(services, authenticationScheme, _ => { });
        public static IServiceCollection AddTokenAuthentication(this IServiceCollection services, Action<TokenAuthenticationOptions> configureOptions) =>
            AddTokenAuthentication(services, AuthenticationTypeDefault, configureOptions);

        public static IServiceCollection AddTokenAuthentication(this IServiceCollection services) =>
            AddTokenAuthentication(services, AuthenticationTypeDefault, _ => { });

        public static IServiceCollection AddTokenAuthentication(this IServiceCollection services, string authenticationScheme, Action<TokenAuthenticationOptions> configureOptions)
        {
            services.AddSingleton<TokenAuthenticationHandler>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<TokenAuthenticationOptions>, TokenAuthenticationPostConfigureOptions>());
            return services.AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(authenticationScheme, authenticationScheme, configureOptions);
        }
    }
}
