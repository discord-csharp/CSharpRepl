using System;
using System.IO;

namespace CSDiscordService
{
    public class Globals
    {
        //public ConsoleLikeStringWriter Console { get; set; }
        public Random Random { get; set; }
        public ConsoleLikeStringWriter Console { get; internal set; }
    }

}