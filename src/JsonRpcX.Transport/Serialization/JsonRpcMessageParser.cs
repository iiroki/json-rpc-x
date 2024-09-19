using System.Text.Json;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Domain.Models;
using JsonRpcX.Transport.Extensions;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Transport.Serialization;

internal class JsonRpcMessageParser(ILogger<JsonRpcMessageParser> logger, JsonSerializerOptions? opt = null)
    : IJsonRpcMessageParser<byte[]>,
        IJsonRpcMessageParser<string>,
        IJsonRpcMessageParser<JsonElement>,
        IJsonRpcMessageParser<JsonRpcRequest>,
        IJsonRpcMessageParser<JsonRpcResponse>
{
    private readonly JsonSerializerOptions? _opt = opt;
    private readonly ILogger _logger = logger;

    public (JsonRpcRequest?, JsonRpcResponse?) Parse(byte[] chunk) => Parse(ReadAsJson(chunk));

    public (JsonRpcRequest?, JsonRpcResponse?) Parse(string chunk) => Parse(ReadAsJson(chunk));

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

    public (JsonRpcRequest?, JsonRpcResponse?) Parse(JsonRpcRequest chunk) => (chunk, null);

    public (JsonRpcRequest?, JsonRpcResponse?) Parse(JsonRpcResponse chunk) => (null, chunk);

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
