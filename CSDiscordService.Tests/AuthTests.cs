using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CSDiscordService
{
    public class AuthTests : IDisposable
    {

        private static readonly JsonSerializerSettings JsonSettings =
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        private ITestOutputHelper Log { get; }

        private TestServer Server { get; set; }

        public AuthTests(ITestOutputHelper outputHelper)
        {
            Environment.SetEnvironmentVariable("tokens", "test;token2;token3");
            var host = new WebHostBuilder()
                .UseApplicationInsights()
                .UseStartup<Startup>();

            Server = new TestServer(host);
            Log = outputHelper;
        }

        [Fact]
        public async Task Auth_BadTokenWithGoodSchemeReturns401()
        {
            using (var client = GetClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", "bad");
                var (result, statusCode) = await Execute(client, "1+1");
                Assert.Equal(HttpStatusCode.Unauthorized, statusCode);
            }
        }

        [Fact]
        public async Task Auth_BadSchemeWithGoodTokenReturns401()
        {
            using (var client = GetClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Thing", "test");
                var (result, statusCode) = await Execute(client, "1+1");
                Assert.Equal(HttpStatusCode.Unauthorized, statusCode);
            }
        }

        [Fact]
        public async Task Auth_GoodTokenAndGoodSchemeReturn200()
        {
            using (var client = GetClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", "test");
                var (result, statusCode) = await Execute(client, "1+1");
                Assert.Equal(HttpStatusCode.OK, statusCode);
            }
        }
        
        [Theory]
        [InlineData("test")]
        [InlineData("token2")]
        [InlineData("token3")]
        public async Task Auth_EachTokenWorks(string token)
        {
            using (var client = GetClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
                var (result, statusCode) = await Execute(client, "1+1");
                Assert.Equal(HttpStatusCode.OK, statusCode);
            }
        }
        
        private HttpClient GetClient()
        {
            var client = new HttpClient(Server.CreateHandler(), true);
            client.BaseAddress = new Uri("http://localhsot");
            return client;
        }

        private async Task<(EvalResult, HttpStatusCode)> Execute(HttpClient client, string expr)
        {
            using (var response = await client.PostAsPlainTextAsync("/eval", expr))
            {
                EvalResult result = null;
                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        result = JsonConvert.DeserializeObject<EvalResult>(content, JsonSettings);
                    }
                    else
                    {
                        Log.WriteLine(content);
                        throw new WebException($"Unexpected status code: {response.StatusCode}");
                    }
                }

                return (result, response.StatusCode);
            }
        }

        public void Dispose()
        {
            Server.Dispose();
        }
    }
}
