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
using System.IO;
using System.Text;

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

        private static readonly Random random = new Random();
        public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
        {
            var code = await req.Content.ReadAsStringAsync();
            var sw = Stopwatch.StartNew();

            var eval = CSharpScript.Create(code, Options, typeof(Globals));

            var compilation = eval.GetCompilation().WithAnalyzers(Analyzers);

            var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(Analyzers, CancellationToken.None);
            sw.Stop();
            var compileTime = sw.Elapsed;
            if (!diagnostics.IsEmpty)
            {
                log.Error($"Forbidden token(s) in request: {string.Join(", ", diagnostics.Select(a => a.GetMessage()))}");
                return req.CreateResponse(HttpStatusCode.Forbidden, "Forbidden token in request");
            }

            ScriptState<object> result = null;

            Exception evalException = null;

            var sb = new StringBuilder();
            var textWr = new StringWriter(sb);
            var globals = new Globals
            {
                Console = textWr,
                Random = random
            };

            try
            {
                sw.Restart();
                result = await eval.RunAsync(globals: globals, catchException: (e) => { evalException = e; return true; });
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

            if (result.Exception == null)
            {
                log.Info($"executed '{code}'");
                return req.CreateResponse(HttpStatusCode.OK, new Result(result, sb.ToString(), sw.Elapsed, compileTime, evalException));
            }
            else
            {
                log.Warning($"failed to execute '{code}'");
                return req.CreateResponse(HttpStatusCode.BadRequest, new Result(result, sb.ToString(), sw.Elapsed, compileTime, evalException));
            }
        }

        public class Globals
        {
            public TextWriter Console { get; set; }
            public Random Random { get; set; }
        }

        public class Result
        {
            public Result(ScriptState<object> state, string consoleOut, TimeSpan executionTime, TimeSpan compileTime, Exception ex = null)
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

                ConsoleOut = consoleOut;
                ReturnValue = state.ReturnValue;
                Code = state.Script.Code;
                Exception = state.Exception?.Message ?? ex?.Message;
            }

            public object ReturnValue { get; set; }

            public string Exception { get; set; }

            public string ExceptionType { get; set; }

            public string Code { get; set; }

            public string ConsoleOut { get; set; }

            public TimeSpan ExecutionTime { get; set; }

            public TimeSpan CompileTime { get; set; }
        }
    }
}