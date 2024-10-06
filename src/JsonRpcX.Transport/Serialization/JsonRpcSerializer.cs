using System.Text.Json;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Domain.Models;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Transport.Serialization;

/// <summary>
/// Default JSON RPC serializers.
/// </summary>
internal class JsonRpcSerializer(ILogger<JsonRpcSerializer> logger, JsonSerializerOptions? opt = null)
    : IJsonRpcInSerializer<byte[]>,
        IJsonRpcInSerializer<string>,
        IJsonRpcInSerializer<JsonElement>,
        IJsonRpcInSerializer<JsonRpcRequest>,
        IJsonRpcInSerializer<JsonRpcResponse>,
        IJsonRpcOutSerializer<byte[]>,
        IJsonRpcOutSerializer<string>,
        IJsonRpcOutSerializer<JsonElement>,
        IJsonRpcOutSerializer<JsonRpcResponse>
{
    private readonly JsonSerializerOptions? _opt = opt;
    private readonly ILogger _logger = logger;

    //
    // Bytes
    //

    public (JsonRpcRequest?, JsonRpcResponse?) Parse(byte[] chunk) => Parse(ReadAsJson(chunk));

    public byte[]? Serialize(JsonRpcResponse? response) =>
        response != null ? JsonSerializer.SerializeToUtf8Bytes(response, _opt) : null;

    //
    // Strings
    //

    public (JsonRpcRequest?, JsonRpcResponse?) Parse(string chunk) => Parse(ReadAsJson(chunk));

    string? IJsonRpcOutSerializer<string>.Serialize(JsonRpcResponse? response) =>
        response != null ? JsonSerializer.Serialize(response, _opt) : null;

    //
    // JSON elements
    //

    public (JsonRpcRequest?, JsonRpcResponse?) Parse(JsonElement chunk)
    {
        JsonRpcRequest? req = null;
        JsonRpcResponse? res = null;

        if (IsJsonRpcRequest(chunk))
        {
            try
            {
                req = chunk.Deserialize<JsonRpcRequest>(_opt);
            }
            catch (JsonException ex)
            {
                throw new JsonRpcInvalidRequestException(ex.Message);
            }
        }
        else if (IsJsonRpcResponse(chunk))
        {
            try
            {
                res = chunk.Deserialize<JsonRpcResponse>(_opt);
            }
            catch (Exception ex)
            {
                // Response parse errors are not thrown!
                _logger.LogWarning(ex, "Could not not parse JSON RPC response");
            }
        }
        else
        {
            throw new JsonRpcInvalidRequestException();
        }

        return (req, res);
    }

    JsonElement IJsonRpcOutSerializer<JsonElement>.Serialize(JsonRpcResponse? response) =>
        JsonSerializer.SerializeToElement(response, _opt);

    //
    // JSON RPC types
    //

    public (JsonRpcRequest?, JsonRpcResponse?) Parse(JsonRpcRequest chunk) => (chunk, null);

    public (JsonRpcRequest?, JsonRpcResponse?) Parse(JsonRpcResponse chunk) => (null, chunk);

    JsonRpcResponse? IJsonRpcOutSerializer<JsonRpcResponse>.Serialize(JsonRpcResponse? response) => response;

    //
    // Helpers
    //

    private JsonElement ReadAsJson(object chunk)
    {
        try
        {
            return chunk switch
            {
                byte[] bytes => JsonSerializer.Deserialize<JsonElement>(bytes, _opt),
                string text => JsonSerializer.Deserialize<JsonElement>(text, _opt),
                JsonElement json => json,
                _ => throw new ArgumentException("Could not read as JSON"),
            };
        }
        catch (JsonException)
        {
            throw new JsonRpcParseErrorException("Unknown object");
        }
    }

    private static bool IsJsonRpcRequest(JsonElement chunk) => chunk.TryGetProperty("method", out _);

    private static bool IsJsonRpcResponse(JsonElement chunk) => chunk.TryGetProperty("result", out _);
}
