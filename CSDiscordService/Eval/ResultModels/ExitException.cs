using System;

namespace CSDiscordService.Eval.ResultModels
{
    public class ExitException : Exception
    {
        public ExitException(int exitCode) : base($"Script exited with code {exitCode}") { }
    }
}
