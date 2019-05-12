using System;
using System.Diagnostics;
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
            System.Environment.Exit(0);
        }

        public void PowerButton()
        {
            System.Environment.Exit(1);
        }
        public void Cmd(string name, string args = "")
        {
            var psi = new ProcessStartInfo(name);
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.Arguments = args;

            var p = Process.Start(psi);
            p.WaitForExit();
            Console.WriteLine(p.StandardOutput.ReadToEnd());
            Console.WriteLine(p.StandardError.ReadToEnd());
        }

    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class ʘ‿ʘAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class ʘ_ʘAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class ಠ_ಠAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class ಠ‿ಠAttribute : Attribute { }
    
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class 눈ᆺ눈Attribute : Attribute { }

}