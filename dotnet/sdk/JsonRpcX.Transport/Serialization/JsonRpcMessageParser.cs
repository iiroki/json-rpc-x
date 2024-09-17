using System.Text.Json;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Domain.Models;
using JsonRpcX.Transport.Extensions;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Transport.Serialization;

internal class JsonRpcMessageParser(JsonSerializerOptions opt, ILogger<JsonRpcMessageParser> logger)
    : IJsonRpcMessageParser<byte[]>,
        IJsonRpcMessageParser<string>,
        IJsonRpcMessageParser<JsonElement>
{
    private readonly JsonSerializerOptions _opt = opt;
    private readonly ILogger _logger = logger;

    public (JsonRpcRequest?, JsonRpcResponse?) Parse(byte[] chunk) =>
        Parse(JsonSerializer.Deserialize<JsonElement>(chunk, _opt));

    public (JsonRpcRequest?, JsonRpcResponse?) Parse(string chunk) => Parse(chunk.GetUtf8Bytes());

    public (JsonRpcRequest?, JsonRpcResponse?) Parse(JsonElement chunk)
    {
        JsonRpcRequest? req = null;
        JsonRpcResponse? res = null;

        if (chunk.TryGetProperty("method", out _))
        {
            try
            {
                req = chunk.Deserialize<JsonRpcRequest>(_opt);
            }
            catch (JsonException ex)
            {
                throw new JsonRpcParseException(ex.Message);
            }
        }
        else
        {
            try
            {
                res = chunk.Deserialize<JsonRpcResponse>(_opt);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not not deserialize JSON RPC response");
            }
        }

        return (req, res);
    }
}
