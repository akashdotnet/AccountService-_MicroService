using System.Text;
using Newtonsoft.Json;

namespace AccountService.API.Utils.Extensions;

public static class ConverterUtility
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    public static byte[]? ObjectToByteArray<T>(this T source)
    {
        if (source == null)
        {
            return null;
        }

        string resultString = JsonConvert.SerializeObject(source, SerializerSettings);
        return Encoding.Unicode.GetBytes(resultString);
    }

    public static T? ByteArrayToObject<T>(this byte[] source)
    {
        string resultString = Encoding.Unicode.GetString(source);
        return JsonConvert.DeserializeObject<T>(resultString);
    }
}
