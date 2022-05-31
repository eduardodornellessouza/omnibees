using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace OB.BL.Operations.Helper
{
    public static class JsonSerializer
    {
        public static string SerializeToJson<T>(List<T> data)
        {
            string contentJson = string.Empty;
            MemoryStream memoryStreamJSon = new MemoryStream();
            var serializer = new DataContractJsonSerializer(typeof(List<T>));
            serializer.WriteObject(memoryStreamJSon, data);
            memoryStreamJSon.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(memoryStreamJSon);
            contentJson = reader.ReadToEnd();
            memoryStreamJSon.Close();
            memoryStreamJSon.Dispose();
            return contentJson;
        }

        public static T DeserializeFromJson<T>(string json)
        {
            if (json == null || json.Length == 0)
            {
                return default(T);
            }
            else
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                {

                    return (T)serializer.ReadObject(stream);

                }
            }
        }

        public static string SerializeToJsonSingle<T>(T data)
        {
            string contentJson = string.Empty;
            MemoryStream memoryStreamJSon = new MemoryStream();
            var serializer = new DataContractJsonSerializer(typeof(T));
            serializer.WriteObject(memoryStreamJSon, data);
            memoryStreamJSon.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(memoryStreamJSon);
            contentJson = reader.ReadToEnd();
            memoryStreamJSon.Close();
            memoryStreamJSon.Dispose();
            return contentJson;
        }
    }
}
