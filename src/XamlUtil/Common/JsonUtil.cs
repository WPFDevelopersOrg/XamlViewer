#if NETFRAMEWORK
using Newtonsoft.Json;
#endif

using System.Text.Json;

namespace XamlUtil.Common
{
    public static class JsonUtil
    {
        public static T DeserializeObject<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default(T);

#if NETFRAMEWORK
            return JsonConvert.DeserializeObject<T>(json);
#else
            return JsonSerializer.Deserialize<T>(json);
#endif

        }
    }
}
