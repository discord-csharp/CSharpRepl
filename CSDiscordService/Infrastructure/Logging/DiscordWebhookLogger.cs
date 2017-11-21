using System;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using Discord;
using Discord.Rest;

namespace CSDiscordService.Infrastructure.Logging
{
    internal class DiscordWebhookLogger : ILogger
    {
        private string _categoryName;
        private string _token;
        private Func<string, LogLevel, bool> _filter;
        private ulong _id;
        private static readonly HttpClient _httpClient = new HttpClient();

        public DiscordWebhookLogger(string categoryName, ulong id, string token, Func<string, LogLevel, bool> filter)
        {
            _categoryName = categoryName;
            _token = token;
            _filter = filter;
            _id = id;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if(!_filter(_categoryName, logLevel))
            {
                return;
            }

            var webhookClient = new Discord.Webhook.DiscordWebhookClient(_id, _token);

            var message = new EmbedBuilder()
                .WithAuthor("DiscordLogger")
                .WithTitle(_categoryName)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithColor(Color.Red)
                .AddField(new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName($"LogLevel: {logLevel}")
                    .WithValue(Format.Code($"{formatter(state, exception)}\n{exception?.ToString()}")));

            webhookClient.SendMessageAsync(string.Empty, embeds: new[] { message.Build() }, username: "CSDiscord Logger");
        }
    }
}