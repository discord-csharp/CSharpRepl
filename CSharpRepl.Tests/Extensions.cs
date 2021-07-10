using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CSDiscordService.Tests
{
    public static class Extensions
    {
        internal static async Task<HttpResponseMessage> PostAsPlainTextAsync(this HttpClient client, string url, string text)
        {
            using var httpContent = new StringContent(text ?? string.Empty, Encoding.UTF8, "text/plain");
            var result = await client.PostAsync(url, httpContent);

            return result;
        }
    }
}
