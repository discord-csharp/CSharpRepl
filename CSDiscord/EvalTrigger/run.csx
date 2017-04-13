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
using System.Diagnostics;
using System.Runtime.ExceptionServices;

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
    "Process(",
    "Thread.",
    "Thread(",
    "File.",
    "Directory.",
    "StreamReader(",
    "StreamWriter(",
    "Environment.",
    "WebClient(",
    "HttpClient(",
    "WebRequest.",
    "Activator.CreateInstance",
    "Invoke(",
    "unsafe",
    "DllImport(",
    "Assembly.Load",
    ".DynamicInvoke",
    ".CreateDelegate",
    "Expression.Call",
    ".Compile()",
    ".Emit(",
    "MethodRental.",
    "using",
    "InvokeMember(",
    "InvokeMethod(",
    ".Socket",
    "Socket("
};


public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    var code = await req.Content.ReadAsStringAsync();

    if (BlockedTokens.Any(a => code.Contains(a)))
    {
        log.Error("Forbidden token in request");
        return req.CreateResponse(HttpStatusCode.Forbidden, "Class not allowed");
    }
    
    ScriptState<object> result = null;

    Exception evalException = null;
    Stopwatch sw = null;

    try
    {
        sw = Stopwatch.StartNew();
        result = await new Runner().Run(code);
        sw.Stop();
    }
    catch (Exception ex)
    {
        evalException = ex;
        log.Error(ex.ToString());
    }
    finally
    {
        if (sw?.IsRunning ?? false)
        {
            sw?.Stop();
        }
    }

    if (result != null && result.Exception == null)
    {
        log.Info($"executed '{code}'");
        return req.CreateResponse(HttpStatusCode.OK, new Result(result, sw.Elapsed, evalException));
    }
    else
    {
        log.Warning($"failed to execute '{code}'");
        return req.CreateResponse(HttpStatusCode.BadRequest, new Result(result, sw.Elapsed, evalException));
    }
}

public class Runner
{
    public async Task<ScriptState<object>> Run(string code)
    {
        var options = ScriptOptions.Default
              .WithImports(DefaultImports)
              .WithReferences(DefaultReferences);
              
        return await CSharpScript.RunAsync(code, options);
    }
}

public class Result
{
    public Result(ScriptState<object> state, TimeSpan executionTime, Exception ex = null)
    {
        if (state == null && ex == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        ExecutionTime = executionTime;

        if (state == null)
        {
            Exception = ex.Message;
            ExceptionType = ex.GetType().Name;
            return;
        }

        ReturnValue = state.ReturnValue;
        Code = state.Script.Code;
        Exception = state.Exception?.Message ?? ex?.Message;
    }

    public object ReturnValue { get; set; }

    public string Exception { get; set; }

    public string ExceptionType { get; set; }

    public string Code { get; set; }

    public TimeSpan ExecutionTime { get; set; }
}