using System.Text.Json;
using JsonRpcX.Extensions;
using JsonRpcX.Models;

namespace JsonRpcX.Core.Requests;

internal class JsonRpcRequestParser(JsonSerializerOptions opt)
    : IJsonRpcRequestParser<byte[]>,
        IJsonRpcRequestParser<string>,
        IJsonRpcRequestParser<JsonElement?>
{
    private readonly JsonSerializerOptions? _opt = opt;

    public JsonRpcRequest? Parse(byte[] chunk) => JsonSerializer.Deserialize<JsonRpcRequest>(chunk, _opt);

    public JsonRpcRequest? Parse(string chunk) => Parse(chunk.GetUtf8Bytes());

    public JsonRpcRequest? Parse(JsonElement? chunk) => chunk?.Deserialize<JsonRpcRequest>(_opt);
}
