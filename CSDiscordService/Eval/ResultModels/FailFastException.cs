using System;

namespace CSDiscordService.Eval.ResultModels
{
    class FailFastException : Exception
    {
        public FailFastException(string message, Exception exception) : base($"Script fast-failed with message \"{message}\"", exception) { }
    }
}
