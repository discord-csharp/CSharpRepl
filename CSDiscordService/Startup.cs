using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using CSDiscordService.Middleware;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

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
            services.AddTransient<Eval>();
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

            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    var exception = JsonConvert.SerializeObject(ex, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = new DefaultContractResolver()
                    });

                    var exBytes = Encoding.UTF8.GetBytes(exception);
                    context.Response.StatusCode = 500;
                    await context.Response.Body.WriteAsync(exBytes, 0, exBytes.Length);
                }
            });

            app.UseMvc();
        }
    }
}
