using System;
using CSDiscordService.Eval.ResultModels;

namespace CSDiscordService.Eval
{
    public class Globals
    {
        //public ConsoleLikeStringWriter Console { get; set; }
        public Random Random { get; set; }
        public ConsoleLikeStringWriter Console { get; internal set; }
    }

}