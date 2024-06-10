using System.Text.Json.Serialization;
using System.Text.Json;

namespace DataStream;
internal class DBNullJsonConverter : JsonConverter<DBNull>
{
    public override DBNull Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DBNull.Value;
    }

    public override void Write(Utf8JsonWriter writer, DBNull value, JsonSerializerOptions options)
    {
        writer.WriteNullValue();
    }
}
