using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using AngouriMath;

namespace CSDiscordService.Eval
{
    public class ScriptExecutionContext
    {
        private static readonly List<string> DefaultImports =
            new()
            {
                "Newtonsoft.Json",
                "Newtonsoft.Json.Linq",
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
            };

        private static readonly List<Assembly> DefaultReferences =
            new()
            {
                typeof(Enumerable).GetTypeInfo().Assembly,
                typeof(HttpClient).GetTypeInfo().Assembly,
                typeof(List<>).GetTypeInfo().Assembly,
                typeof(string).GetTypeInfo().Assembly,
                typeof(Unsafe).GetTypeInfo().Assembly,
                typeof(ValueTuple).GetTypeInfo().Assembly,
                typeof(Globals).GetTypeInfo().Assembly,
                typeof(Memory<>).GetTypeInfo().Assembly,
                typeof(Entity).GetTypeInfo().Assembly,
                typeof(INumber<>).GetTypeInfo().Assembly
        };
        public ScriptOptions Options =>
            ScriptOptions.Default
            .WithLanguageVersion(LanguageVersion.Preview)
            .WithImports(Imports)
            .WithReferences(References);

        public HashSet<Assembly> References { get; private set; } = new HashSet<Assembly>(DefaultReferences);

        public HashSet<string> Imports { get; private set; } = new HashSet<string>(DefaultImports);

        public string Code { get; set; }

        public ScriptExecutionContext(string code)
        {
            Code = code;
        }

        public void AddImport(string import)
        {
            if(string.IsNullOrEmpty(import))
            {
                return;
            }

            if (Imports.Contains(import))
            {
                return;
            }

            Imports.Add(import);
        }

        public bool TryAddReferenceAssembly(Assembly assembly)
        {
            if(assembly is null)
            {
                return false;
            }

            if(References.Contains(assembly))
            {
                return false;
            }

            References.Add(assembly);
            return true;
        }
    }
}
