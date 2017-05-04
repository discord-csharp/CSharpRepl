using System;
using System.IO;

namespace CSDiscordService
{
    public class Globals
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Random random = new Random();

        public Globals(TextWriter consoleWriter)
        {
            if (consoleWriter == null)
                throw new ArgumentNullException(nameof(consoleWriter));

            __Console = consoleWriter;
        }

        // ReSharper disable once InconsistentNaming
        public TextWriter __Console { get; }
        public Random Random => random;
    }

}