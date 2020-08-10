using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

#if NETCOREAPP
using Microsoft.Extensions.DependencyInjection;
#endif

namespace XamlUtil.Net
{
    public class HttpUtil
    {
#if NETCOREAPP
        private static readonly HttpUtil _instance = new HttpUtil();
        private readonly IHttpClientFactory _httpClientFactory = null;

        private HttpUtil() 
        {
            var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            _httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        }

        public static HttpUtil Instance
        {
            get { return _instance; }
        }

        public async Task<string> GetString(string urlString)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
#else
        public static async Task<string> GetString(string urlString)
        {
            using (var httpClient = new HttpClient())
#endif
            {
                return await httpClient.GetString(urlString);
            }
        }
    }

    public static class HttpClientExtensions
    {
        public static async Task<string> GetString(this HttpClient httpClient, string urlString)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            httpClient.DefaultRequestHeaders.Add("accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.9 Safari/537.36");

            return await httpClient.GetStringAsync(urlString);
        }
    }
}
