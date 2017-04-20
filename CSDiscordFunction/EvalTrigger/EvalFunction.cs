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
            var sw = Stopwatch.StartNew();
            var eval = CSharpScript.Create(code, Options);
            var compilation = eval.GetCompilation().WithAnalyzers(Analyzers);
            var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(Analyzers, CancellationToken.None);
            sw.Stop();
            var compileTime = sw.Elapsed;
            if (!diagnostics.IsEmpty)
            {
                log.Error("Forbidden token in request");
                return req.CreateResponse(HttpStatusCode.Forbidden, "Class not allowed");
            }

            ScriptState<object> result = null;

            Exception evalException = null;
            
            try
            {
                sw.Restart();
                result = await eval.RunAsync();
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
                return req.CreateResponse(HttpStatusCode.OK, new Result(result, sw.Elapsed, compileTime, evalException));
            }
            else
            {
                log.Warning($"failed to execute '{code}'");
                return req.CreateResponse(HttpStatusCode.BadRequest, new Result(result, sw.Elapsed, compileTime, evalException));
            }
        }

        public class Result
        {
            public Result(ScriptState<object> state, TimeSpan executionTime, TimeSpan compileTime, Exception ex = null)
            {
                if (state == null && ex == null)
                {
                    throw new ArgumentNullException(nameof(state));
                }

                ExecutionTime = executionTime;
                CompileTime = compileTime;

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

            public TimeSpan CompileTime { get; set; }
        }
    }
}