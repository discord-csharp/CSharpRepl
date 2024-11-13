using System;
using System.Diagnostics;
using CSDiscordService.Eval.ResultModels;

namespace CSDiscordService.Eval
{
    public class Globals
    {
        public static ConsoleLikeStringWriter Console { get; internal set; }

        public void Cmd(string name, string args = "")
        {
            var psi = new ProcessStartInfo(name)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = args
            };

            var p = Process.Start(psi);
            p.WaitForExit();
            Console.WriteLine(p.StandardOutput.ReadToEnd());
            Console.WriteLine(p.StandardError.ReadToEnd());
        }
    }
}