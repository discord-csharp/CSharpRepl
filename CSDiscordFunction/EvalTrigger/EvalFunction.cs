using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.Azure.WebJobs.Host;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using System.Web.Http;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSDiscordFunction
{
    public class EvalFunction
    {

        private static readonly string[] DefaultImports =
        {
            "System",
            "System.IO",
            "System.Linq",
            "System.Collections.Generic",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Net",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Net.Http"
        };

        private static readonly Assembly[] DefaultReferences =
        {
            typeof(Enumerable).Assembly,
            typeof(List<string>).Assembly,
            typeof(JsonConvert).Assembly,
            typeof(HttpConfiguration).Assembly,
            typeof(string).Assembly
        };

        private static readonly ScriptOptions Options =
            ScriptOptions.Default
                .WithImports(DefaultImports)
                .WithReferences(DefaultReferences);

        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new BlacklistedTypesAnalyzer());

        public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
        {
            var code = await req.Content.ReadAsStringAsync();
            var compilation = CSharpScript.Create(code, Options).GetCompilation().WithAnalyzers(Analyzers);
            var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(Analyzers, CancellationToken.None);
            if (!diagnostics.IsEmpty)
            {
                log.Error("Forbidden token in request");
                return req.CreateResponse(HttpStatusCode.Forbidden, "Class not allowed");
            }

            ScriptState<object> result = null;

            Exception evalException = null;
            Stopwatch sw = null;

            try
            {
                sw = Stopwatch.StartNew();
                result = await new Runner().Run(code);
                sw.Stop();
            }
            catch (Exception ex)
            {
                evalException = ex;
                log.Error(ex.ToString());
            }
            finally
            {
                if (sw?.IsRunning ?? false)
                {
                    sw?.Stop();
                }
            }

            if (result != null && result.Exception == null)
            {
                log.Info($"executed '{code}'");
                return req.CreateResponse(HttpStatusCode.OK, new Result(result, sw.Elapsed, evalException));
            }
            else
            {
                log.Warning($"failed to execute '{code}'");
                return req.CreateResponse(HttpStatusCode.BadRequest, new Result(result, sw.Elapsed, evalException));
            }
        }

        public class Runner
        {
            public async Task<ScriptState<object>> Run(string code)
            {
                return await CSharpScript.RunAsync(code, Options);
            }
        }

        public class Result
        {
            public Result(ScriptState<object> state, TimeSpan executionTime, Exception ex = null)
            {
                if (state == null && ex == null)
                {
                    throw new ArgumentNullException(nameof(state));
                }

                ExecutionTime = executionTime;

                if (state == null)
                {
                    Exception = ex.Message;
                    ExceptionType = ex.GetType().Name;
                    return;
                }

                ReturnValue = state.ReturnValue;
                Code = state.Script.Code;
                Exception = state.Exception?.Message ?? ex?.Message;
            }

            public object ReturnValue { get; set; }

            public string Exception { get; set; }

            public string ExceptionType { get; set; }

            public string Code { get; set; }

            public TimeSpan ExecutionTime { get; set; }
        }
    }
}