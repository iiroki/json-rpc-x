using System.Text.Json;
using System.Text.Json.Serialization;
using JsonRpcX.Models;

namespace JsonRpcX.Serialization;

public class JsonRpcResponseConverter : JsonConverter<JsonRpcResponse>
{
    public override JsonRpcResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions opt) =>
        // TODO: Implement when receiving responses
        throw new NotSupportedException($"\"{nameof(JsonRpcResponse)}\" JSON deserialization is not supported");

    public override void Write(Utf8JsonWriter writer, JsonRpcResponse value, JsonSerializerOptions opt)
    {
        if (value.IsSuccess)
        {
            var converter = (JsonConverter<JsonRpcResponseSuccess>)opt.GetConverter(typeof(JsonRpcResponseSuccess));
            converter.Write(writer, value.Success, opt);
        }
        else
        {
            var converter = (JsonConverter<JsonRpcResponseError>)opt.GetConverter(typeof(JsonRpcResponseError));
            converter.Write(writer, value.Error, opt);
        }
    }
}
