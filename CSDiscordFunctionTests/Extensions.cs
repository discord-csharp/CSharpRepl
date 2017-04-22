using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace CSDiscordFunctionTests
{
    public static class Extensions
    {
        public static string AsString(this HttpResponseMessage msg)
        {
            return msg.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static HttpRequestMessage AsRequest(this string content)
        {
            var request = new HttpRequestMessage(new HttpMethod("POST"), "/")
            {
                Content = new StringContent(content, Encoding.UTF8, "text/plain")
            };
            request.SetConfiguration(new HttpConfiguration());
            return request;
        }
    }
}
