using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using CSDiscordService.Eval.ResultModels;
using Microsoft.AspNetCore;

namespace CSDiscordService.Tests
{
    public class EvalTests : IDisposable
    {
        private static readonly JsonSerializerSettings JsonSettings =
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            
        public EvalTests(ITestOutputHelper outputHelper)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentName.Development);

            var host = WebHost.CreateDefaultBuilder()
                //.UseApplicationInsights()
                .UseStartup<Startup>();
                //.ConfigureServices(a => a.AddSingleton(_dummyTelemetryClient));

            Log = outputHelper;
            Server = new TestServer(host);
            Client = Server.CreateClient();
        }

        private ITestOutputHelper Log { get; }

        private TestServer Server { get; }

        private HttpClient Client { get; }

        [Theory]
        [InlineData("1+1", 2L, "int")]
        [InlineData("return 1+1;", 2L, "int")]
        [InlineData("return Random.Next(1,2);", 1L, "int")]
        [InlineData(@"var a = ""thing""; return a;", "thing", "string")]
        [InlineData("Math.Pow(1,2)", 1D, "double")]
        [InlineData(@"Enumerable.Range(0,1).Select(a=>""@"");", null, null)]
        public async Task Eval_WellFormattedCodeExecutes(string expr, object expected, string type)
        {
            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal(expected, result.ReturnValue);
            Assert.Equal(type, result.ReturnTypeName);
        }

        [Theory]
        [InlineData(@"new DirectoryInfo(""C:\\"")", "An exception occurred when serializing the response: JsonSerializationException: Unable to serialize instance of 'System.IO.DirectoryInfo'.")]
        [InlineData(@"return 4896.ToString().Select(Char.GetNumericValue).Cast<int>();", "An exception occurred when serializing the response: InvalidCastException: Unable to cast object of type 'System.Double' to type 'System.Int32'.")]
        public async Task Eval_JsonNetSerializationFailureHandled(string expr, string message)
        {
            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal(message, result.ReturnValue);
        }

        [Theory]
        [InlineData(@"Enumerable.Range(0,1).Select(a=>""@"")", "@", 1, "List<string>")]
        [InlineData(@"return Enumerable.Range(0,1).Select(a=>""@"");", "@", 1, "List<string>")]
        public async Task Eval_EnumerablesReturnArraysOf(string expr, object expected, int count, string type)
        {
            var (result, statusCode) = await Execute(expr);

            var res = result.ReturnValue as JArray;
            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal(expected, res[0].Value<string>());
            Assert.Equal(count, res.Count);
            Assert.Equal(type, result.ReturnTypeName);
        }

        [Theory]
        [InlineData("return 1+1", "CompilationErrorException", "; expected")]
        [InlineData(@"throw new Exception(""test"");", "Exception", "test")]
        [InlineData("return System.Environment.MachineName;", "CompilationErrorException", "Usage of this API is prohibited\nUsage of this API is prohibited")]
        [InlineData("return DoesNotCompile()", "CompilationErrorException", "; expected\nThe name 'DoesNotCompile' does not exist in the current context")]
        public async Task Eval_FaultyCodeThrowsExpectedException(string expr, string exception, string message)
        {
            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.BadRequest, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal(exception, result.ExceptionType);
            Assert.Equal(message, result.Exception);
        }

        [Theory]
        [InlineData(@"Console.WriteLine(""test"");", "test\r\n", null)]
        [InlineData(@"public static void Test() {Console.WriteLine(""Test"");} Test();", "Test\r\n", null)]
        public async Task Eval_ConsoleOutputIsCaptured(string expr, string consoleOut, object returnValue)
        {
            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal(consoleOut, result.ConsoleOut);
            Assert.Equal(returnValue, result.ReturnValue);
        }
        
        [Fact]
        public async Task Eval_ConsoleOutputIsCapturedAndValueReturned()
        {
            var expr = @"Console.WriteLine(""test""); return ""abcdefg"";";
            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal("test\r\n", result.ConsoleOut);
            Assert.Equal("abcdefg", result.ReturnValue);
            Assert.Equal("string", result.ReturnTypeName);
        }

        [Fact]
        public async Task Eval_AsyncMethodBuilderTestCompileFails()
        {
            var expr = @"[AsyncMethodBuilder(typeof(Builder))]
                    class async
                    {
                        public Awaiter GetAwaiter() => new Awaiter();

                        public class Awaiter : INotifyCompletion
                        {
                            public void GetResult() { }
                            public bool IsCompleted => false;
                            public void OnCompleted(Action a) { }
                        }

                        class Builder { }
                    }

                    class Foo
                    {
                        async async async(async async) => await async;
                    }";

            var (result, statusCode) = await Execute(expr);
            Assert.Equal(HttpStatusCode.BadRequest, statusCode);
        }

        [Fact]
        public async Task Eval_CSharp71Supported()
        {
            var expr = @"int thing = default; return thing;";
            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal(0L, result.ReturnValue);
            Assert.Equal("int", result.ReturnTypeName);
        }

        [Theory]
        [InlineData(@"var a = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(""D""), AssemblyBuilderAccess.Run);
            var b = a.DefineDynamicModule(""D"");
            var c = b.DefineType(""DO"", TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.AutoClass | TypeAttributes.Abstract | TypeAttributes.Sealed);
            var d = c.DefineMethod(""AddUp"", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.Static, CallingConventions.Standard, typeof(int), new[] { typeof(int), typeof(int) });
            var e = d.GetILGenerator();
            e.Emit(OpCodes.Ldarg_0);
            e.Emit(OpCodes.Ldarg_1);
            e.Emit(OpCodes.Add);
            e.Emit(OpCodes.Ret);
            var f = c.CreateTypeInfo().GetMethod(""AddUp"").Invoke(null, new object[] { 1, 2 });
            Console.WriteLine(""1 + 2: {0}"", f);
            Console.ReadLine(); ")]
        public async Task Eval_BlackCentipedesBlackMagicWorks(string code)
        {
            var expr = code;

            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.OK, statusCode);
        }

        [Fact]
        public async Task Eval_CSharp72Supported()
        {
            var expr = @"public class BaseClass
                        {
                            private protected int myValue = 42;
                        }
                        public class DerivedClass1 : BaseClass
                        {
                            public int Access()
                            {
                                return myValue;
                            }
                        }
                        return new DerivedClass1().Access();";

            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal(42L, result.ReturnValue);
            Assert.Equal("int", result.ReturnTypeName);
        }
        
        [Fact]
        public async Task Eval_CSharp80Supported()
        {
            var expr = @"public class BaseClass
                        {
                            public string? myValue = null;
                        }

                        return new BaseClass().myValue;";

            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Null(result.ReturnValue);
            Assert.Null(result.ReturnTypeName);
        }

        [Fact]
        public async Task Eval_CanUseSystemDrawing()
        {
            var expr = @"Console.WriteLine(System.Drawing.Color.Red);";
            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal("Color [Red]\r\n", result.ConsoleOut);
        }

        private async Task<(EvalResult, HttpStatusCode)> Execute(string expr)
        {
            using (var response = await Client.PostAsPlainTextAsync("http://testhost/eval", expr))
            {
                EvalResult result = null;
                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Log.WriteLine(content.Replace(@"\r\n", Environment.NewLine));

                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        result = JsonConvert.DeserializeObject<EvalResult>(content, JsonSettings);
                    }
                }

                return (result, response.StatusCode);
            }
        }

        public void Dispose()
        {
            Client.Dispose();
            Server.Dispose();
        }
    }
}
