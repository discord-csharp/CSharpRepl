using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CSDiscordService.Eval
{
    public class DisassemblyService
    {
        private static readonly IReadOnlyCollection<MetadataReference> References = ImmutableArray.Create(
        MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
        MetadataReference.CreateFromFile(typeof(ValueTuple<>).GetTypeInfo().Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),
        MetadataReference.CreateFromFile(typeof(List<>).GetTypeInfo().Assembly.Location),
        MetadataReference.CreateFromFile(typeof(JsonConvert).GetTypeInfo().Assembly.Location),
        MetadataReference.CreateFromFile(typeof(string).GetTypeInfo().Assembly.Location),
        MetadataReference.CreateFromFile(typeof(HttpClient).GetTypeInfo().Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Regex).GetTypeInfo().Assembly.Location),
        MetadataReference.CreateFromFile(typeof(BinaryExpression).GetTypeInfo().Assembly.Location)
    );

        private static readonly ImmutableArray<string> Imports = ImmutableArray.Create(
            "System",
            "System.IO",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Collections.Generic",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Net",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Net.Http",
            "Newtonsoft.Json",
            "Newtonsoft.Json.Linq",
            "System.Reflection",
            "System.Reflection.Emit"
        );

        public string GetIl(string code)
        {
            string toExecute = $@"
            namespace Eval
            {{
              public class Code
              {{
                public object Main() 
                {{
                  {code}
                }}
              }}
            }}
            ";

            var opts = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest).WithKind(SourceCodeKind.Regular);

            var scriptSyntaxTree = CSharpSyntaxTree.ParseText(toExecute, opts);
            var compOpts = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithOptimizationLevel(OptimizationLevel.Debug).WithAllowUnsafe(true).WithPlatform(Platform.AnyCpu);

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString(), options: compOpts, references: References).AddSyntaxTrees(scriptSyntaxTree);

            var sb = new StringBuilder();
            using (var dll = new MemoryStream())
            {
                var result = compilation.Emit(dll);
                if (!result.Success)
                {
                    sb.AppendLine("Emit Failed");
                    sb.AppendLine(string.Join(Environment.NewLine, result.Diagnostics.Select(a => a.GetMessage())));
                }
                else
                {
                    dll.Seek(0, SeekOrigin.Begin);
                    using (var module = ModuleDefinition.ReadModule(dll))
                    using (var writer = new StringWriter(sb))
                    {
                        module.Name = compilation.AssemblyName;
                        var plainOutput = new PlainTextOutput(writer);
                        var rd = new ReflectionDisassembler(plainOutput, CancellationToken.None)
                        {
                            DetectControlStructure = false
                        };
                        var methods = module.Types.SelectMany(a => a.Methods);
                        rd.DisassembleMethod(methods.Single(a => a.Name == "Main"));
                    }
                }

                var final = sb.ToString();

                return final;
            }
        }
    }
}
