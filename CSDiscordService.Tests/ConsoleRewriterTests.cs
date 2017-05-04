using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace CSDiscordService
{
    public class ConsoleRewriterTests
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

        [Fact]
        public async Task Works()
        {
            const string Code =
                "public static class Console\n" +
                "{\n" +
                "    public static void WriteLine(string s) =>\n" +
                "        global::System.Console.WriteLine(s);\n" +
                "}\n" +
                "\n" +
                "Console.WriteLine(\"Hello, world!\");\n";

            var script = CSharpScript.Create(Code, Options, typeof(Globals));
            var compilation = script.GetCompilation();

            var newTrees = new List<SyntaxNode>();
            foreach (var tree in compilation.SyntaxTrees)
            {
                var model = compilation.GetSemanticModel(tree);

                var rewriter = new ConsoleRewriter(model);
                var newTree = rewriter.Visit(tree.GetRoot());

                newTrees.Add(newTree);
            }

            Script<object> newScript = null;
            foreach (var tree in newTrees)
            {
                var code = tree.ToString();
                if (newScript == null)
                {
                    newScript = CSharpScript.Create(code, Options, typeof(Globals));
                }
                else
                {
                    newScript.ContinueWith(code);
                }
            }

            var builder = new StringBuilder();
            using (var textWriter = new StringWriter(builder))
            {
                await newScript.RunAsync(new Globals(textWriter));
                await textWriter.FlushAsync();
            }

            Assert.NotEmpty(builder.ToString());
        }
    }
}
