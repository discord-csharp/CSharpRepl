using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSDiscordService.Eval
{
    public class ScriptExecutionContext
    {
        private static readonly IEnumerable<string> DefaultImports =
        [
            "System",
            "System.Collections",
            "System.Collections.Concurrent",
            "System.Collections.Immutable",
            "System.Collections.Generic",
            "System.Diagnostics",
            "System.Dynamic",
            "System.Security.Cryptography",
            "System.Globalization",
            "System.IO",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Net",
            "System.Net.Http",
            "System.Numerics",
            "System.Reflection",
            "System.Reflection.Emit",
            "System.Runtime.CompilerServices",
            "System.Runtime.InteropServices",
            "System.Runtime.Intrinsics",
            "System.Runtime.Intrinsics.X86",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Text.Json",
            "CSDiscordService.Eval",
            "AngouriMath",
            "AngouriMath.Extensions",
            "HonkSharp.Fluency",
            "HonkSharp.Functional"
        ];

        private static readonly IEnumerable<Assembly> DefaultReferences =
        [
            typeof(Enumerable).Assembly,
            typeof(System.Net.Http.HttpClient).Assembly,
            typeof(List<>).Assembly,
            typeof(System.ValueTuple).Assembly,
            typeof(Globals).Assembly,
            typeof(System.Memory<>).Assembly,
            typeof(AngouriMath.Entity).Assembly
        ];

        public ScriptOptions Options =>
            ScriptOptions.Default
                .WithLanguageVersion(LanguageVersion.Preview)
                .WithImports(Imports)
                .WithReferences(References);

        public HashSet<Assembly> References { get; } = DefaultReferences.ToHashSet();

        public HashSet<string> Imports { get; } = DefaultImports.ToHashSet();

        public string Code { get; set; }

        public ScriptExecutionContext(string code)
        {
            Code = code;
        }
    }
}
