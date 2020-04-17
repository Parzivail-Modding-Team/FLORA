using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace FLORA
{
    static class JsonUtil
    {
        public static T ParseJson<T>(this string value)
        {
            var ser = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
                return (T)ser.ReadObject(stream);
        }
    }
}
