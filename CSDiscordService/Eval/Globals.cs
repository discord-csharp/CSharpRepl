using System;
using CSDiscordService.Eval.ResultModels;

namespace CSDiscordService.Eval
{
    public class Globals
    {
        public static Random Random { get; set; }
        public static ConsoleLikeStringWriter Console { get; internal set; }
        public static BasicEnvironment Environment { get; internal set; }

        public void ResetButton()
        {
            Environment.Exit(0);
        }

        public void PowerButton()
        {
            Environment.Exit(1);
        }
    }

}