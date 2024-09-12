using System.Text.Json.Serialization;
using JsonRpcX.Domain.Serialization;

namespace JsonRpcX.Domain.Models;

[JsonConverter(typeof(JsonRpcResponseConverter))]
public class JsonRpcResponse
{
    private readonly JsonRpcResponseSuccess? _success;
    private readonly JsonRpcResponseError? _error;

    public JsonRpcResponse(JsonRpcResponseSuccess success)
    {
        _success = success;
    }

    public JsonRpcResponse(JsonRpcResponseError error)
    {
        _error = error;
    }

    public string? Id => IsSuccess ? Success.Id : Error.Id;

    public bool IsSuccess => _success != null;

    public JsonRpcResponseSuccess Success =>
        _success ?? throw new InvalidOperationException($"\"{nameof(IsSuccess)}\" must be true");

    public JsonRpcResponseError Error =>
        _error ?? throw new InvalidOperationException($"\"{nameof(IsSuccess)}\" must be false");
}
