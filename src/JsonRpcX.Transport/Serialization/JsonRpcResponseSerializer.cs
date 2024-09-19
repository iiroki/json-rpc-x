using System.Text.Json;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Transport.Serialization;

/// <summary>
/// Default JSON RPC response serializers.
/// </summary>
internal class JsonRpcResponseSerializer(JsonSerializerOptions? opt = null)
    : IJsonRpcResponseSerializer<byte[]>,
        IJsonRpcResponseSerializer<string>,
        IJsonRpcResponseSerializer<JsonElement>,
        IJsonRpcResponseSerializer<JsonRpcResponse>
{
    private readonly JsonSerializerOptions? _opt = opt;

    //
    // Bytes
    //

    public byte[]? Serialize(JsonRpcResponse? response) =>
        response != null ? JsonSerializer.SerializeToUtf8Bytes(response, _opt) : null;

    //
    // Strings
    //

    string? IJsonRpcResponseSerializer<string>.Serialize(JsonRpcResponse? response) =>
        response != null ? JsonSerializer.Serialize(response, _opt) : null;

    //
    // JSON elements
    //

    JsonElement IJsonRpcResponseSerializer<JsonElement>.Serialize(JsonRpcResponse? response) =>
        JsonSerializer.SerializeToElement(response, _opt);

    //
    // JSON RPC response
    //

    JsonRpcResponse? IJsonRpcResponseSerializer<JsonRpcResponse>.Serialize(JsonRpcResponse? response) => response;
}
