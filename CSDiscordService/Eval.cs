using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using System.Net.Http;

namespace CSDiscordService
{
    public class Eval
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
            typeof(Enumerable).GetTypeInfo().Assembly,
            typeof(List<string>).GetTypeInfo().Assembly,
            typeof(JsonConvert).GetTypeInfo().Assembly,
            typeof(string).GetTypeInfo().Assembly,
            typeof(ValueTuple).GetTypeInfo().Assembly,
            typeof(HttpClient).GetTypeInfo().Assembly
        };

        private static readonly ScriptOptions Options =
            ScriptOptions.Default
                .WithImports(DefaultImports)
                .WithReferences(DefaultReferences);

        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new BlacklistedTypesAnalyzer());

        public async Task<EvalResult> RunEvalAsync(string code)
        {
            var sb = new StringBuilder();
            var textWr = new StringWriter(sb);

            var sw = Stopwatch.StartNew();
            var eval = CSharpScript.Create(code, Options, typeof(Globals));
            var compilation = eval.GetCompilation().WithAnalyzers(Analyzers);
            var compileResult = await compilation.GetAllDiagnosticsAsync();
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
                    ConsoleOut = sb.ToString(),
                    Exception = ex.Message,
                    ExceptionType = ex.GetType().Name,
                    ExecutionTime = TimeSpan.FromMilliseconds(0),
                    ReturnValue = null,
                    ReturnTypeName = null
                };
            }

            var globals = new Globals(textWr);

            sw.Restart();
            var result = await eval.RunAsync(globals, ex => true);
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