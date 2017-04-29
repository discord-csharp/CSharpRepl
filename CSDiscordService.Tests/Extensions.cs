using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CSDiscordService
{
    public static class Extensions
    {
        internal static async Task<HttpResponseMessage> PostAsPlainTextAsync(this HttpClient client, string url, string text)
        {
            using (var httpContent = new StringContent(text ?? string.Empty, Encoding.UTF8, "text/plain"))
            {
                return await client.PostAsync(url, httpContent);
            }
        }
    }
}
