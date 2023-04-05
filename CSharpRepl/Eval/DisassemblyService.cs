using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
using ICSharpCode.Decompiler.Metadata;

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
            MetadataReference.CreateFromFile(typeof(BinaryExpression).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location)
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
            var imports = new StringBuilder();
            foreach(var import in Imports) 
            {
                imports.AppendLine($"using {import};");
            }

            string toExecute = $@"
            {imports}

            namespace Eval
            {{
              public unsafe class Code
              {{
                public object Main() 
                {{
                  {code}
                }}
              }}
            }}
            ";

            var opts = CSharpParseOptions.Default
                .WithLanguageVersion(LanguageVersion.Preview)
                .WithKind(SourceCodeKind.Regular);

            var scriptSyntaxTree = CSharpSyntaxTree.ParseText(toExecute, opts);
            var compOpts = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Debug)
                .WithAllowUnsafe(true)
                .WithPlatform(Platform.AnyCpu);

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString(), options: compOpts, references: References)
                .AddSyntaxTrees(scriptSyntaxTree);

            var sb = new StringBuilder();
            using var dll = new MemoryStream();
            var result = compilation.Emit(dll);
            if (!result.Success)
            {
                sb.AppendLine("Emit Failed");
                sb.AppendLine(string.Join(Environment.NewLine, result.Diagnostics.Select(a => a.GetMessage())));
            }
            else
            {
                dll.Seek(0, SeekOrigin.Begin);
                using var file = new PEFile(compilation.AssemblyName!, dll);
                using var writer = new StringWriter(sb);
                var plainOutput = new PlainTextOutput(writer);
                var rd = new ReflectionDisassembler(plainOutput, CancellationToken.None)
                {
                    DetectControlStructure = true
                };
                var ignoredMethods = new[] { ".ctor" };
                var methods = file.Metadata.MethodDefinitions.Where(a =>
                {
                    var methodName = file.Metadata.GetString(file.Metadata.GetMethodDefinition(a).Name);
                    return !ignoredMethods.Contains(methodName);
                });
                foreach (var method in methods)
                {
                    rd.DisassembleMethod(file, method);
                    plainOutput.WriteLine();
                }
            }

            var final = sb.ToString();

            return final;
        }
    }
}
