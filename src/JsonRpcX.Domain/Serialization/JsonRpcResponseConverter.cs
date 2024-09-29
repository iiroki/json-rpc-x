using System.Buffers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Domain.Serialization;

/// <summary>
/// JSON converter for JSON RPC responses.
/// </summary>
internal class JsonRpcResponseConverter : JsonConverter<JsonRpcResponse>
{
    private static readonly string ResultProperty = GetResultPropertyName();

    public override JsonRpcResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions opt)
    {
        JsonRpcResponseSuccess? success = null;
        JsonRpcResponseError? error = null;

        var elementConverter = GetElementConverter(opt);
        var json = elementConverter.Read(ref reader, typeof(JsonElement), opt);
        var isSuccess = json.TryGetProperty(ResultProperty, out _);

        if (isSuccess)
        {
            success = json.Deserialize<JsonRpcResponseSuccess>(opt);
        }
        else
        {
            error = json.Deserialize<JsonRpcResponseError>(opt);
        }

        if (success != null)
        {
            return new JsonRpcResponse(success);
        }
        else if (error != null)
        {
            return new JsonRpcResponse(error);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, JsonRpcResponse value, JsonSerializerOptions opt)
    {
        if (value.IsSuccess)
        {
            var converter = GetSuccessConverter(opt);
            converter.Write(writer, value.Success, opt);
        }
        else
        {
            var converter = GetErrorConverter(opt);
            converter.Write(writer, value.Error, opt);
        }
    }

    private static JsonConverter<JsonElement> GetElementConverter(JsonSerializerOptions opt) =>
        (JsonConverter<JsonElement>)opt.GetConverter(typeof(JsonElement));

    private static JsonConverter<JsonRpcResponseSuccess> GetSuccessConverter(JsonSerializerOptions opt) =>
        (JsonConverter<JsonRpcResponseSuccess>)opt.GetConverter(typeof(JsonRpcResponseSuccess));

    private static JsonConverter<JsonRpcResponseError> GetErrorConverter(JsonSerializerOptions opt) =>
        (JsonConverter<JsonRpcResponseError>)opt.GetConverter(typeof(JsonRpcResponseError));

    private static string GetResultPropertyName()
    {
        var @default = nameof(JsonRpcResponseSuccess.Result);
        var prop = typeof(JsonRpcResponseSuccess).GetProperty(@default);
        var attr = prop?.GetCustomAttribute<JsonPropertyNameAttribute>();
        return attr?.Name ?? @default;
    }
}
