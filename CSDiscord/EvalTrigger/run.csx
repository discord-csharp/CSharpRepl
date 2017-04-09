#r "System.Collections"
#r "System.Runtime"
#r "System.Reflection"
#r "System.Threading.Tasks"
#r "System.Net.Http"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.Azure.WebJobs.Host;
using System.Web;

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
    "System.Threading.Tasks"
};

private static readonly Assembly[] DefaultReferences =
{
    typeof(Enumerable).Assembly,
    typeof(List<string>).Assembly
};

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    var code = await req.Content.ReadAsStringAsync();
    object result = null;
    var successful = false;
    try
    {
        result = await CSharpScript.EvaluateAsync(code,
            ScriptOptions.Default
                .WithImports(DefaultImports)
                .WithReferences(new[] {
                    "System",
                    "System.Core",
                    "System.Xml",
                    "System.Xml.Linq",
                })
                .WithReferences(DefaultReferences));
        successful = true;
    }
    catch (Exception ex)
    {
        log.Error(ex.ToString());
        result = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
    }

    var resultText = result?.ToString() ?? "null";

    if (successful)
    {
        return req.CreateResponse(HttpStatusCode.OK, result);
    }
    else
    {
        return req.CreateResponse(HttpStatusCode.BadRequest, result);
    }
}
