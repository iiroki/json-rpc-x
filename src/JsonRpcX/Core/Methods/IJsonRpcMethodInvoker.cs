using System.Reflection;
using System.Text.Json;
using JsonRpcX.Controllers;

namespace JsonRpcX.Core.Methods;

/// <summary>
/// Interface for invoking a JSON RPC method against a JSON RPC controller.
/// </summary>
internal interface IJsonRpcMethodInvoker
{
    /// <summary>
    /// JSON RPC method handler to invoke the methods against.
    /// </summary>
    public IJsonRpcController Controller { get; }

    /// <summary>
    /// JSON RPC method invocation metadata.
    /// </summary>
    public MethodInfo Method { get; }

    /// <summary>
    /// Invokes the JSON RPC method against the controller.
    /// </summary>
    public Task<object?> InvokeAsync(JsonElement? @params, CancellationToken ct = default);
}
