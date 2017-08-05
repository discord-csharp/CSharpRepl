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
using System.Net.Http.Headers;

namespace CSDiscordService
{
    public class EvalTests : IDisposable
    {
        private static readonly JsonSerializerSettings JsonSettings =
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        public EvalTests(ITestOutputHelper outputHelper)
        {
            Environment.SetEnvironmentVariable("tokens", "test");
            var host = new WebHostBuilder()
                .UseApplicationInsights()
                .UseStartup<Startup>();

            Log = outputHelper;
            Server = new TestServer(host);
            Client = Server.CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", "test");
        }

        private ITestOutputHelper Log { get; }

        private TestServer Server { get; }

        private HttpClient Client { get; }

        [Theory]
        [InlineData("1+1", 2L, "Int32")]
        [InlineData("return 1+1;", 2L, "Int32")]
        [InlineData("return Random.Next(1,2);", 1L, "Int32")]
        [InlineData(@"var a = ""thing""; return a;", "thing", "String")]
        [InlineData("Math.Pow(1,2)", 1D, "Double")]
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
        [InlineData(@"Enumerable.Range(0,1).Select(a=>""@"")", "@", 1, "List<String>")]
        [InlineData(@"return Enumerable.Range(0,1).Select(a=>""@"");", "@", 1, "List<String>")]
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
        [InlineData("return Environment.MachineName;", "CompilationErrorException", "Usage of this API is prohibited")]
        [InlineData("return DoesNotCompile()", "CompilationErrorException", "; expected\nThe name 'DoesNotCompile' does not exist in the current context")]
        public async Task Eval_FaultyCodeThrowsExpectedException(string expr, string exception, string message)
        {
            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.BadRequest, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal(exception, result.ExceptionType);
            Assert.Equal(message, result.Exception);
        }

        [Fact]
        public async Task Eval_ConsoleOutputIsCaptured()
        {
            var expr = @"Console.WriteLine(""test"");";
            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal("test\r\n", result.ConsoleOut);
            Assert.Null(result.ReturnValue);
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
            Assert.Equal("String", result.ReturnTypeName);
        }

        [Fact]
        public async Task Eval_CSharp71Supported()
        {
            var expr = @"int thing = default; return thing;";
            var (result, statusCode) = await (Execute(expr));

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal(0L, result.ReturnValue);
            Assert.Equal("Int32", result.ReturnTypeName);
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
            using (var response = await Client.PostAsPlainTextAsync("/eval", expr))
            {
                EvalResult result = null;
                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Log.WriteLine(content);

                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        result = JsonConvert.DeserializeObject<EvalResult>(content, JsonSettings);
                    }
                    else
                    {
                        throw new WebException($"Unexpected status code: {response.StatusCode}");
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
