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

private static readonly string[] BlockedTokens =
{
    "Process.",
    "Thread.",
    "Thread(",
    "File.",
    "Directory.",
    "StreamReader(",
    "StreamWriter(",
    "Environment.",
    "WebClient(",
    "HttpClient("
};

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    var code = await req.Content.ReadAsStringAsync();
    
    if (BlockedTokens.Any(a => code.Contains(a)))
    {
        log.Error("Forbidden token in request");
        return req.CreateResponse(HttpStatusCode.Forbidden, "Class not allowed");
    }

    object result = null;
    var successful = false;
    var options = ScriptOptions.Default
                .WithImports(DefaultImports)
                .WithReferences(DefaultReferences);
                
    try
    {
        var cts = new System.Threading.CancellationTokenSource(5000);
        result = await CSharpScript.EvaluateAsync(code, options, cancellationToken: cts.Token);
        successful = true;
    }
    catch (Exception ex)
    {
        log.Error(ex.ToString());
        result = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
    }

    if (successful)
    {
        log.Info($"executed '{code}'");
        return req.CreateResponse(HttpStatusCode.OK, result);
    }
    else
    {
        log.Warning($"failed to execute '{code}");
        return req.CreateResponse(HttpStatusCode.BadRequest, result);
    }
}
