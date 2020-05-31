using Newtonsoft.Json;

namespace XamlUtil.Common
{
    public static class JsonUtil
    {
        public static T DeserializeObject<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default(T);

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
