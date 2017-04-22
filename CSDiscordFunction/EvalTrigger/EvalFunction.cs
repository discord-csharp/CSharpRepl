using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using System.Security.Policy;
using System.Security;
using System.Collections.Generic;

namespace CSDiscordFunction
{
    public class EvalFunction
    {
        static EvalFunction()
        {
            string assemblyBase = ResolveActualAssemblyPath() + ".config";

            var setup = new AppDomainSetup()
            {
                ConfigurationFile = assemblyBase,
                ApplicationBase = Path.GetDirectoryName(assemblyBase),
                ApplicationName = "Sandbox",
                DisallowCodeDownload = true,
                DisallowPublisherPolicy = true,
                DisallowBindingRedirects = false,
            };

            var evidence = new Evidence();
            evidence.AddHostEvidence(new Zone(SecurityZone.Untrusted));

            var permissions = AppDomain.CurrentDomain.PermissionSet;

            domain = AppDomain.CreateDomain("Sandbox", evidence, setup, permissions);

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            AppDomain.CurrentDomain.DomainUnload += (s, e) =>
            {
                AppDomain.Unload(domain);
            };
        }

        private static readonly AppDomain domain;

        public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
        {
            log.Info(ResolveActualAssemblyPath());

            var evalType = typeof(Eval);
            var eval = (Eval)domain.CreateInstanceAndUnwrap(evalType.Assembly.FullName, evalType.FullName);
            var code = await req.Content.ReadAsStringAsync();
            var result = eval.RunEval(code);

            if (result.Exception == null)
            {
                log.Info($"Executed {code}");
            }
            else
            {
                log.Warning($"Failed to execute {code}");
            }

            return req.CreateResponse(result.Exception == null ? HttpStatusCode.OK : HttpStatusCode.BadRequest, Result.FromEvalResult(result));
        }

        private static Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;
            var assemblyBase = ResolveActualAssemblyPath();

            var parts = args.Name.Split(',');
            var file = $"{Path.GetDirectoryName(assemblyBase)}\\{parts[0].Trim()}.dll";
            assembly = Assembly.LoadFrom(file);
            return assembly;
        }

        private static string ResolveActualAssemblyPath()
        {
            var assemblyBase = Assembly.GetExecutingAssembly().Location;
            if (assemblyBase.Contains(Environment.GetEnvironmentVariable("temp")))
            {
                var assemblyFile = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
                assemblyBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyFile);
            }
            Console.WriteLine($"Resolved Assembly path to {assemblyBase}");
            return assemblyBase;
        }

    }
}