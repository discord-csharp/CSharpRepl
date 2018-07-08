using Microsoft.Extensions.Logging;
using System;

namespace CSDiscordService.Infrastructure.Logging
{
    internal class DiscordWebhookLoggingProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly string _token;
        private readonly ulong _webhookId;

        public DiscordWebhookLoggingProvider(Func<string, LogLevel, bool> filter, ulong id, string token)
        {
            _filter = filter;
            _token = token;
            _webhookId = id;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DiscordWebhookLogger(categoryName, _webhookId, _token, _filter);
        }

        public void Dispose()
        {
        }
    }
}