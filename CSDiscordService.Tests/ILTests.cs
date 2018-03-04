using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit.Abstractions;
using Microsoft.ApplicationInsights;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using System.Net;

namespace CSDiscordService
{
    public class ILTests : IDisposable
    {

        private static readonly JsonSerializerSettings JsonSettings =
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        public static TelemetryClient _dummyTelemetryClient = new TelemetryClient();


        public ILTests(ITestOutputHelper outputHelper)
        {
            var host = new WebHostBuilder()
                .UseSetting("tokens", "test")
                .UseStartup<Startup>()
                .ConfigureServices(a => a.AddSingleton(_dummyTelemetryClient));

            Log = outputHelper;
            Server = new TestServer(host);
            Client = Server.CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", "test");
        }

        private ITestOutputHelper Log { get; }

        private TestServer Server { get; }

        private HttpClient Client { get; }


        [Theory]
        [InlineData("return 1+1;")]
        [InlineData("int Thing() {return 1+1;} return Thing();")]
        [InlineData("void N(){ ref int M(out int x){x = 10; return ref x;}ref int i = ref M(out _);}N(); return 0;")]
        [InlineData("1+1")]
        //[InlineData("System.Console.WriteLine(\"Hi\");")] //disabled until i can figure out why it won't 'Console'
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
