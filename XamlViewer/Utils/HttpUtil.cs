using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace XamlViewer.Utils
{
    public static class HttpUtil
    {
        public static async Task<string> GetString(string urlString)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("accept", "*/*");
                httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.9 Safari/537.36");

                using (var response = await httpClient.GetAsync(urlString).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return null;
                }
            } 
        }
    }
}
