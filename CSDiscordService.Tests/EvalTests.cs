using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace CSDiscordService
{
    public class EvalTests : IDisposable
    {
        private static readonly JsonSerializerSettings JsonSettings =
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        public EvalTests(ITestOutputHelper outputHelper)
        {
            var config = new ConfigurationBuilder()
                .Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Startup>();

            Log = outputHelper;
            Server = new TestServer(host);
            Client = Server.CreateClient();
        }

        private ITestOutputHelper Log { get; }

        private TestServer Server { get; }

        private HttpClient Client { get; }

        [Theory]
        [InlineData("1+1", 2L)]
        [InlineData("return 1+1;", 2L)]
        [InlineData("return Random.Next(1,2);", 1L)]
        [InlineData(@"var a = ""thing""; return a;", "thing")]
        [InlineData("Math.Pow(1,2)", 1D)]
        [InlineData(@"Enumerable.Range(0,1).Select(a=>""@"");", null)]
        public async Task Eval_WellFormattedCodeExecutes(string expr, object expected)
        {
            var (result, statusCode) = await Execute(expr);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal(expected, result.ReturnValue);
        }

        [Theory]
        [InlineData(@"Enumerable.Range(0,1).Select(a=>""@"")", "@", 1)]
        [InlineData(@"return Enumerable.Range(0,1).Select(a=>""@"");", "@", 1)]
        public async Task Eval_EnuymerablesReturnArraysOf(string expr, object expected, int count)
        {
            var (result, statusCode) = await Execute(expr);

            Assert.IsType<object[]>(result.ReturnValue);

            var res = result.ReturnValue as object[];
            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal(expected, res[0]);
            Assert.Equal(count, res.Length);
        }


        [Theory]
        [InlineData("return 1 +1", "CompilationErrorException", "; expected")]
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
            var (result, statusCode) = await (Execute(expr));

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal("test\r\n", result.ConsoleOut);
            Assert.Null(result.ReturnValue);
        }

        [Fact]
        public async Task Eval_ConsoleOutputIsCapturedAndValueReturned()
        {
            var expr = @"Console.WriteLine(""test""); return ""abcdefg"";";
            var (result, statusCode) = await (Execute(expr));

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(expr, result.Code);
            Assert.Equal("test\r\n", result.ConsoleOut);
            Assert.Equal("abcdefg", result.ReturnValue);
        }

        private async Task<(EvalResult, HttpStatusCode)> Execute(string expr)
        {
            using (var response = await Client.PostAsPlainTextAsync("/eval", expr))
            {
                EvalResult result = null;
                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        result = JsonConvert.DeserializeObject<EvalResult>(content, JsonSettings);
                    }
                    else
                    {
                        Log.WriteLine(content);
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
