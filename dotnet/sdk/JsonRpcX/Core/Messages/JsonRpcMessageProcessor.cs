using System.Text.Json;
using JsonRpcX.Exceptions;
using JsonRpcX.Extensions;
using JsonRpcX.Models;

namespace JsonRpcX.Core.Messages;

internal class JsonRpcMessageProcessor(
    IServiceProvider services,
    JsonSerializerOptions jsonOptions,
    IJsonRpcExceptionHandler? exceptionHandler = null
) : JsonRpcMessageProcessorBase<byte[], byte[]?>(services, exceptionHandler)
{
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions;

    protected override byte[]? ConvertToOutputType(JsonRpcResponse? response) =>
        response != null ? JsonSerializer.Serialize(response, _jsonOptions).GetUtf8Bytes() : null;
}
