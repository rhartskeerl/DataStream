using System.Text.Json;

namespace DataStream;
internal class Constants
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new DBNullJsonConverter() }
    };
}
