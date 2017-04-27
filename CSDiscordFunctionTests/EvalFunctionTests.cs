//using CSDiscordFunction.EvalTrigger;
//using System.Diagnostics;
//using System.Threading.Tasks;
//using Xunit;
//using Newtonsoft.Json;
//using System.Net;
//using System.Collections.Generic;

//namespace CSDiscordFunctionTests
//{
//    public class EvalFunctionTests
//    {
//        [Theory]
//        [InlineData("1+1", 2L)]
//        [InlineData("return 1+1;", 2L)]
//        [InlineData("return Random.Next(1,2);", 1L)]
//        [InlineData(@"var a = ""thing""; return a;", "thing")]
//        [InlineData("Math.Pow(1,2)", 1D)]
//        [InlineData(@"Enumerable.Range(0,1).Select(a=>""@"");", null)]
//        public async Task Eval_WellFormattedCodeExecutes(string expr, object expected)
//        {
//            var (result, statusCode) = await Execute(expr);

//            Assert.Equal(HttpStatusCode.OK, statusCode);
//            Assert.Equal(expr, result.Code);
//            Assert.Equal(expected, result.ReturnValue);
//        }

//        [Theory]
//        [InlineData(@"Enumerable.Range(0,1).Select(a=>""@"")", "@", 1)]
//        [InlineData(@"return Enumerable.Range(0,1).Select(a=>""@"");", "@", 1)]
//        public async Task Eval_EnuymerablesReturnArraysOf(string expr, object expected, int count)
//        {
//            var (result, statusCode) = await Execute(expr);

//            Assert.IsType<object[]>(result.ReturnValue);

//            var res = result.ReturnValue as object[];
//            Assert.Equal(HttpStatusCode.OK, statusCode);
//            Assert.Equal(expr, result.Code);
//            Assert.Equal(expected, res[0]);
//            Assert.Equal(count, res.Length);
//        }


//        [Theory]
//        [InlineData("return 1 +1", "CompilationErrorException", "; expected")]
//        [InlineData(@"throw new Exception(""test"");", "Exception", "test")]
//        [InlineData("return Environment.MachineName;", "CompilationErrorException", "Usage of this API is prohibited")]
//        [InlineData("return DoesNotCompile()", "CompilationErrorException", "; expected\nThe name 'DoesNotCompile' does not exist in the current context")]
//        public async Task Eval_FaultyCodeThrowsExpectedException(string expr, string exception, string message)
//        {
//            var (result, statusCode) = await Execute(expr);

//            Assert.Equal(HttpStatusCode.BadRequest, statusCode);
//            Assert.Equal(expr, result.Code);
//            Assert.Equal(exception, result.ExceptionType);
//            Assert.Equal(message, result.Exception);
//        }

//        [Fact]
//        public async Task Eval_ConsoleOutputIsCaptured()
//        {
//            var expr = @"Console.WriteLine(""test"");";
//            var (result, statusCode) = await (Execute(expr));

//            Assert.Equal(HttpStatusCode.OK, statusCode);
//            Assert.Equal(expr, result.Code);
//            Assert.Equal("test\r\n", result.ConsoleOut);
//            Assert.Null(result.ReturnValue);
//        }

//        [Fact]
//        public async Task Eval_ConsoleOutputIsCapturedAndValueReturned()
//        {
//            var expr = @"Console.WriteLine(""test""); return ""abcdefg"";";
//            var (result, statusCode) = await (Execute(expr));

//            Assert.Equal(HttpStatusCode.OK, statusCode);
//            Assert.Equal(expr, result.Code);
//            Assert.Equal("test\r\n", result.ConsoleOut);
//            Assert.Equal("abcdefg", result.ReturnValue);
//        }

//        private async Task<(Result, HttpStatusCode)> Execute(string expr)
//        {
//            ;

//            var result = await EvalFunction.Run(request, new DummyWriter(TraceLevel.Off));
//            var resultObj = JsonConvert.DeserializeObject<Result>(result.AsString());
//            return (resultObj, result.StatusCode);
//        }
//    }
//}
