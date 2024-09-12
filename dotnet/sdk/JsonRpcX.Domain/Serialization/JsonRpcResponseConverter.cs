using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Domain.Serialization;

/// <summary>
/// JSON converter for JSON RPC responses.
/// </summary>
internal class JsonRpcResponseConverter : JsonConverter<JsonRpcResponse>
{
    public override JsonRpcResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions opt)
    {
        JsonRpcResponseSuccess? success = null;
        JsonRpcResponseError? error = null;

        var successConverter = GetSuccessConverter(opt);
        if (successConverter.CanConvert(typeof(JsonRpcResponseSuccess)))
        {
            success = successConverter.Read(ref reader, typeof(JsonRpcResponseSuccess), opt);
        }
        else
        {
            var errorConverter = GetErrorConverter(opt);
            if (errorConverter.CanConvert(typeof(JsonRpcResponseError)))
            {
                error = errorConverter.Read(ref reader, typeof(JsonRpcResponseError), opt);
            }
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

    private static JsonConverter<JsonRpcResponseSuccess> GetSuccessConverter(JsonSerializerOptions opt) =>
        (JsonConverter<JsonRpcResponseSuccess>)opt.GetConverter(typeof(JsonRpcResponseSuccess));

    private static JsonConverter<JsonRpcResponseError> GetErrorConverter(JsonSerializerOptions opt) =>
        (JsonConverter<JsonRpcResponseError>)opt.GetConverter(typeof(JsonRpcResponseError));
}
