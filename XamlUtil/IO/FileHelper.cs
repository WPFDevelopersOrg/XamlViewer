using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Utils.IO
{
    public static class FileHelper
    {
        private static bool SaveToFile(string filePath, Action<TextWriter> serialize)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (var sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        serialize(sw);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static T LoadFromFile<T>(string filePath, Func<TextReader, T> deserialize)
        {
            try
            {
                if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
                    return default(T);

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        return deserialize(sr);
                    }
                }
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        public static string ComputeMD5(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ComputeMD5(fs);
            }
        }

        public static string ComputeMD5(FileStream fs)
        {
            if (fs == null)
                return null;

            var md5Provider = new MD5CryptoServiceProvider();
            return string.Join("", md5Provider.ComputeHash(fs).Select(b => b.ToString("X2")).ToArray());
        }

        #region XML

        public static bool SaveToXmlFile<T>(string filePath, T instance)
        {
            return SaveToFile(filePath, sw =>
            {
                new XmlSerializer(typeof(T)).Serialize(sw, instance);
            });
        }

        public static T LoadFromXmlFile<T>(string filePath)
        {
            return LoadFromFile(filePath, sr =>
            {
                return (T)(new XmlSerializer(typeof(T))).Deserialize(sr);
            });
        }

        #endregion

        #region JSON

        public static bool SaveToJsonFile<T>(string filePath, T instance)
        {
            return SaveToFile(filePath, sw =>
            {
                sw.Write(JsonConvert.SerializeObject(instance));
            });
        }

        public static T LoadFromJsonFile<T>(string filePath)
        {
            return LoadFromFile(filePath, sr =>
            {
                return JsonConvert.DeserializeObject<T>(sr.ReadToEnd(), new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                });
            });
        }

        #endregion
    }
}
