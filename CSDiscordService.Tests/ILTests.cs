using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using Xunit.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using Microsoft.AspNetCore;

namespace CSDiscordService.Tests
{
    public class ILTests : IDisposable
    {            
        public ILTests(ITestOutputHelper outputHelper)
        {
            var host = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>();
          
            Log = outputHelper;
            Server = new TestServer(host);
            Client = Server.CreateClient();
        }

        private ITestOutputHelper Log { get; }

        private TestServer Server { get; }

        private HttpClient Client { get; }


        [Theory]
        [InlineData("return 1+1;")]
        [InlineData("int Thing() {return 1+1;} return Thing();")]
        [InlineData("void N(){ ref int M(out int x){x = 10; return ref x;}ref int i = ref M(out _);}N(); return 0;")]
        [InlineData("System.Console.WriteLine(\"Hi\"); return 0;")] 
        public async Task TestIfWorks(string script)
        {
            var (result, code) = await Execute(script);
            Assert.DoesNotContain("Emit Failed", result);
        }

        private async Task<(string, HttpStatusCode)> Execute(string expr)
        {
            using (var response = await Client.PostAsPlainTextAsync("/il", expr))
            {
                string result = string.Empty;
                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Log.WriteLine(content.Replace(@"\r\n", Environment.NewLine));

                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        result = content;
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
