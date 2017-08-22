using System;
using CSDiscordService.Eval.ResultModels;

namespace CSDiscordService.Eval
{
    public class Globals
    {
        public Random Random { get; set; }
        public ConsoleLikeStringWriter Console { get; internal set; }

        public void ResetButton()
        {
            Environment.Exit(0);
        }
    }

}