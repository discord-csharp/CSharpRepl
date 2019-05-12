using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Serialization;
using CSDiscordService.Eval;
using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace CSDiscordService
{
    public class Startup
    {
        private readonly Timer _exitTimer = new Timer((s) => Environment.Exit(0), null, Timeout.Infinite, Timeout.Infinite);
        public Startup(IWebHostEnvironment env, IConfiguration hostBuilderConfig)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddInMemoryCollection(hostBuilderConfig.AsEnumerable())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.EnvironmentName == Environments.Development)
            {
                builder.AddUserSecrets("03629088-8bb9-4faf-8162-debf93066bc4");
               // builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddApplicationInsightsTelemetryProcessor<FilterStatusProbeTelemetryProcessor>();
            services.AddSingleton<CSharpEval>();
            services.AddSingleton<DisassemblyService>();
            services.AddControllers(o =>
            {
                o.RespectBrowserAcceptHeader = true;
                o.InputFormatters.Clear();
                o.InputFormatters.Insert(0, new PlainTextInputFormatter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddNewtonsoftJson();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, CSharpEval evalService)
        {
            // run eval once on startup so the first time its hit isn't cripplingly slow.
            evalService.RunEvalAsync("1+1").ConfigureAwait(false).GetAwaiter().GetResult();
            app.UseRouting();
            app.Use(async (context, next) =>
            {
                if (env.IsProduction() && !context.Response.HasStarted && (context.Request.Path.Equals("/eval", StringComparison.OrdinalIgnoreCase)))
                {
                    // terminate hte process after 30 seconds whether the request is done or not (infinite loops, long sleeps, etc)
                    _exitTimer.Change(TimeSpan.FromSeconds(30), Timeout.InfiniteTimeSpan);

                    // terminate the process when the rquest finishes (assume the code is malicious. 
                    // Should be hosted in a container/host system that destroys/re-builds the container)
                    context.Response.OnCompleted(() =>
                    {
                        Environment.Exit(0);
                        return Task.CompletedTask;
                    });
                }

                await next();
            });

            app.UseEndpoints(o =>
            {
                o.MapControllers();
            });
        }
    }
}
