using CSDiscordService.Eval.ResultModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

namespace CSDiscordService.Eval
{
    public class CSharpEval
    {
        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new BlacklistedTypesAnalyzer());

        private static readonly Random random = new Random();
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly IPreProcessorService _preProcessor;
        private readonly ILogger<CSharpEval> _logger;

        public CSharpEval(JsonSerializerOptions serializerOptons, IPreProcessorService preProcessor, ILogger<CSharpEval> logger)
        {
            _serializerOptions = serializerOptons;
            _preProcessor = preProcessor;
            _logger = logger;
        }

        public async Task<EvalResult> RunEvalAsync(string code)
        {
            var sb = new StringBuilder();
            using var textWr = new ConsoleLikeStringWriter(sb);
            var env = new BasicEnvironment();

            var sw = Stopwatch.StartNew();

            var context = new ScriptExecutionContext(code);
            try
            {
                await _preProcessor.PreProcess(context, s => _logger.LogInformation("{message}",s));
            }
            catch(Exception ex)
            {
                var diagnostics = Diagnostic.Create(new DiagnosticDescriptor("REPL01", ex.Message, ex.Message, "Code", DiagnosticSeverity.Error, true), 
                    Location.Create("", TextSpan.FromBounds(0,0), new LinePositionSpan(LinePosition.Zero, LinePosition.Zero)));
                _logger.LogCritical(ex, "{message}", ex.Message);
                return EvalResult.CreateErrorResult(code, sb.ToString(), sw.Elapsed, new[] { diagnostics }.ToImmutableArray());
            }
            var eval = CSharpScript.Create(context.Code, context.Options, typeof(Globals));

            var compilation = eval.GetCompilation().WithAnalyzers(Analyzers);

            var compileResult = await compilation.GetAllDiagnosticsAsync();
            var compileErrors = compileResult.Where(a => a.Severity == DiagnosticSeverity.Error).ToImmutableArray();
            sw.Stop();

            var compileTime = sw.Elapsed;
            if (!compileErrors.IsEmpty)
            {
                return EvalResult.CreateErrorResult(code, sb.ToString(), sw.Elapsed, compileErrors);
            }

            var globals = new Globals();
            Globals.Random = random;
            Globals.Console = textWr;
            Globals.Environment = env;

            sw.Restart();
            ScriptState<object> result;

            try
            {
                result = await eval.RunAsync(globals, ex => true);
            }
            catch (CompilationErrorException ex)
            {
                return EvalResult.CreateErrorResult(code, sb.ToString(), sw.Elapsed, ex.Diagnostics);
            }
            sw.Stop();

            var evalResult = new EvalResult(code, result, sb.ToString(), sw.Elapsed, compileTime)
            {
                Code = code
            };
            //this hack is to test if we're about to send an object that can't be serialized back to the caller.
            //if the object can't be serialized, return a failure instead.
            try
            {
                _ = JsonSerializer.Serialize(evalResult, _serializerOptions);
            }
            catch (Exception ex)
            {
                evalResult = new EvalResult
                {
                    Code = code,
                    CompileTime = compileTime,
                    ConsoleOut = sb.ToString(),
                    ExecutionTime = sw.Elapsed,
                    Exception = $"An exception occurred when serializing the response: {ex.GetType().Name}: {ex.Message}",
                    ExceptionType = ex.GetType().Name
                };
            }

            return evalResult;
        }
    }
}
