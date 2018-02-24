using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CSDiscordService.Middleware;
using System.Linq;
using Newtonsoft.Json.Serialization;
using CSDiscordService.Eval;

namespace CSDiscordService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration hostBuilderConfig)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddInMemoryCollection(hostBuilderConfig.AsEnumerable())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets("03629088-8bb9-4faf-8162-debf93066bc4");
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<CSharpEval>();
            services.AddSingleton<DisassemblyService>();
            services.AddTokenAuthentication(o => o.ValidTokens = Configuration["tokens"].Split(";").ToList());

            services.AddMvc(o =>
            {
                o.RespectBrowserAcceptHeader = true;
                o.InputFormatters.Clear();
                o.InputFormatters.Add(new PlainTextInputFormatter());
            })
             .AddJsonOptions(o =>
             {
                 o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                 o.SerializerSettings.ContractResolver = new DefaultContractResolver();
             });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            var webhookToken = Configuration["log_webhook_token"];
            if (!string.IsNullOrWhiteSpace(webhookToken))
            {
                var webhookId = ulong.Parse(Configuration["log_webhook_id"]);
                loggerFactory.AddDiscordWebhook(webhookId, webhookToken);
            }
          

            app.UseMvc();
        }
    }
    
}
