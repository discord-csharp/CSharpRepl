using CSDiscordService.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;

namespace CSDiscordService
{
    public static class DiscordWebhookLoggingExtensions
    {
        public static ILoggingBuilder AddDiscordWebhook(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DiscordWebhookLoggingProvider>());
            return builder;
        }

        public static ILoggerFactory AddDiscordWebhook(this ILoggerFactory factory, ulong id, string token, Func<string, LogLevel, bool> filter = null)
        {
            if(filter == null)
            {
                filter = (str, level) => level >= LogLevel.Error;
            }

            factory.AddProvider(new DiscordWebhookLoggingProvider(filter, id, token));
            return factory;
        }
    }
}
