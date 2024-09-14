using System.Reflection;
using System.Text;
using System.Text.Json;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Extensions;
using JsonRpcX.Helpers.Extensions;
using JsonRpcX.Methods;

namespace JsonRpcX.Core.Methods;

internal class JsonRpcMethodInvoker(
    IJsonRpcMethodHandler handler,
    MethodInfo method,
    JsonSerializerOptions? jsonOptions = null
) : IJsonRpcMethodInvoker
{
    public IJsonRpcMethodHandler Handler { get; } = handler;

    public MethodInfo Method { get; } = method;

    private readonly JsonSerializerOptions? _jsonOptions = jsonOptions;

    public async Task<object?> InvokeAsync(JsonElement? @params, CancellationToken ct = default)
    {
        var input = GetParameters(@params, ct);
        var result = Invoke(input);

        // If the result is a task -> Wait for the completion.
        if (result is Task task)
        {
            await task; // This also throws async exceptions!
            if (task.GetType().GenericTypeArguments.Length > 0)
            {
                result = task.IsCompletedSuccessfully
                    ? GetTaskResult(task)
                    : throw new JsonRpcException("Could not extract result from task");
            }
        }

        return result;
    }

    private object? Invoke(object?[]? @params)
    {
        try
        {
            return Method.Invoke(Handler, @params);
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex;
        }
    }

    private object?[]? GetParameters(JsonElement? json, CancellationToken ct)
    {
        var methodParams = Method.GetParameters();

        // Exlude the cancellation token so the actual parameters are easier to handle
        var hasCt = methodParams.LastOrDefault()?.ParameterType == typeof(CancellationToken);
        if (hasCt)
        {
            methodParams = methodParams[0..(methodParams.Length - 1)];
        }

        object?[]? parsed = methodParams.Length > 0 ? ParseParams(json, methodParams) : [];
        return @hasCt ? [.. parsed, ct] : parsed;
    }

    private object? ParseParam(JsonElement? json, ParameterInfo info, int index)
    {
        if (!json.HasValue || json.Value.IsNull())
        {
            var isNullable = info.IsNullable();
            var value = info.HasDefaultValue ? info.DefaultValue : null;
            if (!isNullable && value == null)
            {
                throw new JsonRpcParamException($"Required param missing - Index: {index}");
            }

            return value;
        }

        try
        {
            return json.Value.Deserialize(info.ParameterType, _jsonOptions);
        }
        catch (JsonException)
        {
            throw new JsonRpcParamException(
                $"Invalid param type - Index: {index}, "
                    + $"Expected: {info.ParameterType.Name}, Received: {json.Value.ValueKind}"
            );
        }
    }

    private object?[]? ParseParams(JsonElement? json, ParameterInfo[] info)
    {
        var paramDefaultCount = 0;
        for (var i = info.Length - 1; i >= 0; --i)
        {
            if (info[i].HasDefaultValue)
            {
                ++paramDefaultCount;
            }
            else
            {
                break;
            }
        }

        var paramRequiredCount = info.Length - paramDefaultCount;
        if (info.Length == 1)
        {
            return [ParseParam(json, info.First(), 0)];
        }

        var items = json.HasValue && json.Value.IsArray() ? json.Value.EnumerateArray().ToList() : [];
        if (items.Count < paramRequiredCount || items.Count > info.Length)
        {
            var msgBuilder = new StringBuilder($"Expected \"params\" array with length: {paramRequiredCount}");
            if (paramDefaultCount != 0)
            {
                msgBuilder.Append($" - {paramRequiredCount + paramDefaultCount}");
            }

            throw new JsonRpcParamException(msgBuilder.ToString());
        }

        var @params = new object?[info.Length];
        for (var i = 0; i < @params.Length; ++i)
        {
            JsonElement? el = i < items.Count ? items[i] : null;
            @params[i] = ParseParam(el, info[i], i);
        }

        return @params;
    }

    private static object? GetTaskResult(Task task) =>
        task.GetType().GetProperty(nameof(Task<object>.Result))?.GetValue(task);
}
