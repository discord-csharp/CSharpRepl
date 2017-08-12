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
using System.Text;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.CodeAnalysis.CSharp;
using CSDiscordService.Eval.ResultModels;

namespace CSDiscordService.Eval
{
    public class CSharpEval
    {
        private static readonly ImmutableArray<string> DefaultImports =
            ImmutableArray.Create(
                "System",
                "System.IO",
                "System.Linq",
                "System.Collections.Generic",
                "System.Text",
                "System.Text.RegularExpressions",
                "System.Net",
                "System.Threading",
                "System.Threading.Tasks",
                "System.Net.Http",
                "Newtonsoft.Json",
                "Newtonsoft.Json.Linq"
            );

        private static readonly ImmutableArray<Assembly> DefaultReferences =
            ImmutableArray.Create(
                typeof(Enumerable).GetTypeInfo().Assembly,
                typeof(List<string>).GetTypeInfo().Assembly,
                typeof(JsonConvert).GetTypeInfo().Assembly,
                typeof(string).GetTypeInfo().Assembly,
                typeof(ValueTuple).GetTypeInfo().Assembly,
                typeof(HttpClient).GetTypeInfo().Assembly
            );

        private static readonly ScriptOptions Options =
            ScriptOptions.Default
                .WithImports(DefaultImports)
                .WithReferences(DefaultReferences);

        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new BlacklistedTypesAnalyzer());

        private static readonly Random random = new Random();

        public async Task<EvalResult> RunEvalAsync(string code)
        {
            var sb = new StringBuilder();
            var textWr = new ConsoleLikeStringWriter(sb);

            var sw = Stopwatch.StartNew();
            var eval = CSharpScript.Create(code, Options, typeof(Globals));
            
            var compilation = eval.GetCompilation().WithAnalyzers(Analyzers);

            var compileResult = await compilation.GetAllDiagnosticsAsync();
            var compileErrors = compileResult.Where(a => a.Severity == DiagnosticSeverity.Error).ToImmutableArray();
            sw.Stop();
            
            var compileTime = sw.Elapsed;
            if (compileErrors.Length > 0)
            {
                return EvalResult.CreateErrorResult(code, sb.ToString(), sw.Elapsed, compileErrors);
            }

            var globals = new Globals
            {
                Random = random,
                Console = textWr
            };

            sw.Restart();
            var result = await eval.RunAsync(globals, ex => true);
            sw.Stop();

            return new EvalResult(result, sb.ToString(), sw.Elapsed, compileTime);
        }
    }
}
