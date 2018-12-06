using System;

namespace CSDiscordService.Infrastructure.Logging
{
    public class NotScoped : IDisposable
    {
        public static NotScoped Instance { get; } = new NotScoped();

        private NotScoped()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}