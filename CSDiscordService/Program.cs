using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using System;
using System.Diagnostics;

namespace CSDiscordService
{
    public class Program
    {
        private static Timer _healthcheckTimer;

        public static void Main(string[] args)
        {
            _healthcheckTimer = new Timer(DoHealthcheck, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(30));
            var host = new WebHostBuilder()
                .UseApplicationInsights()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();
            host.Run();
        }

        private static void DoHealthcheck(object state)
        {
            var allocatedBytes = Process.GetCurrentProcess().PrivateMemorySize64;
            if (allocatedBytes > 2147483648L)
            {
                Console.WriteLine($"Allocated bytes is greater than 2gb ({allocatedBytes}), exiting.");
                Environment.Exit(0);
            }
        }
    }
}
