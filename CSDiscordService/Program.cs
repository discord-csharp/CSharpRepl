using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using System;
using System.Diagnostics;
using Microsoft.AspNetCore;

namespace CSDiscordService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder()
                //.UseApplicationInsights()
                .UseStartup<Startup>();
    }
}
