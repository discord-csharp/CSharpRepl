using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Microsoft.Extensions.Logging.Console;

namespace CSDiscordService
{
    public class Program
    {
        public static void Main()
        {
            Environment.SetEnvironmentVariable("HOME", Path.GetTempPath());
            CreateWebHostBuilder().Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder() =>
            WebHost.CreateDefaultBuilder()
                .ConfigureLogging((context, builder) =>
                {
                    builder.ClearProviders();
                    builder.AddDebug();
                    builder.AddConfiguration(context.Configuration);
                    builder.AddSimpleConsole(o =>
                    {
                        o.ColorBehavior = LoggerColorBehavior.Disabled;
                        o.SingleLine = true;
                        o.TimestampFormat = "yyyy-MM-ddThh:mm:ss.zzzz ";
                    });
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddFilter(level => true);
                })
                .UseStartup<Startup>();
    }
}
