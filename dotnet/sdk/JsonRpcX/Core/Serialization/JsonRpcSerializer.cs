using System.Text.Json;
using JsonRpcX.Extensions;
using JsonRpcX.Models;

namespace JsonRpcX.Core.Serialization;

internal class JsonRpcSerializer(JsonSerializerOptions opt)
    : IJsonRpcSerializer<byte[]>,
        IJsonRpcSerializer<string>,
        IJsonRpcSerializer<JsonElement>
{
    private readonly JsonSerializerOptions? _opt = opt;

    //
    // Bytes
    //

    public JsonRpcRequest? Parse(byte[] chunk) => JsonSerializer.Deserialize<JsonRpcRequest>(chunk, _opt);

    public byte[]? Serialize(JsonRpcResponse? response) => Stringify(response)?.GetUtf8Bytes();

    //
    // Strings
    //

    public JsonRpcRequest? Parse(string chunk) => Parse(chunk.GetUtf8Bytes());

    string? IJsonRpcResponseSerializer<string>.Serialize(JsonRpcResponse? response) => Stringify(response);

    //
    // JSON elements
    //

    public JsonRpcRequest? Parse(JsonElement? chunk) => chunk?.Deserialize<JsonRpcRequest>(_opt);

    JsonElement IJsonRpcResponseSerializer<JsonElement>.Serialize(JsonRpcResponse? response) =>
        JsonSerializer.SerializeToElement(response, _opt);

    //
    // Helpers
    //

    private string? Stringify(JsonRpcResponse? response) =>
        response != null ? JsonSerializer.Serialize(response, _opt) : null;

    public JsonRpcRequest? Parse(JsonElement chunk)
    {
        throw new NotImplementedException();
    }
}
