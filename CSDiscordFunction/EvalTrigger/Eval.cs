using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Web.Http;
using Microsoft.CodeAnalysis.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;

namespace CSDiscordFunction
{
    public class Eval : MarshalByRefObject
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

        public EvalResult RunEval(string code)
        {
            var sw = Stopwatch.StartNew();

            var eval = CSharpScript.Create(code, Options, typeof(Globals));

            var compilation = eval.GetCompilation().WithAnalyzers(Analyzers);
            var compileResult = compilation.GetAllDiagnosticsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var compileErrors = compileResult.Where(a => a.Severity == DiagnosticSeverity.Error).ToImmutableArray();

            sw.Stop();
            var compileTime = sw.Elapsed;

            if (compileErrors.Length > 0)
            {
                var ex = new CompilationErrorException(string.Join("\n", compileErrors.Select(a => a.GetMessage())), compileErrors);
                return new EvalResult
                {
                    Code = code,
                    CompileTime = sw.Elapsed,
                    ConsoleOut = string.Empty,
                    Exception = ex.Message,
                    ExceptionType = ex.GetType().Name,
                    ExecutionTime = TimeSpan.FromMilliseconds(0),
                    ReturnValue = null
                };
            }

            ScriptState<object> result = null;

            var sb = new StringBuilder();
            var textWr = new StringWriter(sb);
            var globals = new Globals
            {
                Console = textWr,
                Random = random
            };

            sw.Restart();
            result = eval.RunAsync(globals, ex => true).ConfigureAwait(false).GetAwaiter().GetResult();
            sw.Stop();

            if (result.Exception == null)
            {
                return new EvalResult(result, sb.ToString(), sw.Elapsed, compileTime);
            }
            else
            {
                return new EvalResult(result, sb.ToString(), sw.Elapsed, compileTime);
            }

        }
    }

}