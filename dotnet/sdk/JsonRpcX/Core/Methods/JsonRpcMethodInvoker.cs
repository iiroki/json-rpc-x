using System.Reflection;
using System.Text;
using System.Text.Json;
using JsonRpcX.Exceptions;
using JsonRpcX.Extensions;
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
        var result = Invoke(GetParameters(@params, ct));

        // If the result is a task -> Wait for the completion.
        if (result is Task task)
        {
            await task;
            if (task.GetType().GenericTypeArguments.Length > 0)
            {
                if (task.IsCompletedSuccessfully)
                {
                    result = GetTaskResult(task);
                }
                else if (task.Exception != null)
                {
                    throw task.Exception;
                }
                else
                {
                    throw new JsonRpcException("Could not extract result from a task");
                }
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
        catch (ArgumentException ex)
        {
            throw new JsonRpcParamException($"Invalid param type - {ex.Message}");
        }
    }

    private object?[]? GetParameters(JsonElement? json, CancellationToken ct)
    {
        var methodParams = Method.GetParameters();
        var hasCt = methodParams.LastOrDefault()?.ParameterType == typeof(CancellationToken);
        if (hasCt)
        {
            methodParams = methodParams[0..(methodParams.Length - 1)];
        }

        object?[]? parsedParams;
        if (methodParams.Length > 0)
        {
            parsedParams =
                json.HasValue && json.Value.IsArray()
                    ? ParseMultipleParams(json, methodParams)
                    : [ParseSingleParam(json, methodParams.First(), 0)];
        }
        else
        {
            parsedParams = [];
        }

        return @hasCt ? [.. parsedParams, ct] : parsedParams;
    }

    private object? ParseSingleParam(JsonElement? json, ParameterInfo info, int index)
    {
        if (!json.HasValue || json.Value.IsNull())
        {
            var isNullable = Nullable.GetUnderlyingType(info.ParameterType) != null;
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

    private object?[]? ParseMultipleParams(JsonElement? json, ParameterInfo[] info)
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
        if (paramRequiredCount == 0 && json?.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        if (!json.HasValue || json.Value.ValueKind != JsonValueKind.Array)
        {
            var msgBuilder = new StringBuilder($"Expected \"params\" array with length: {paramRequiredCount}");
            if (paramDefaultCount != 0)
            {
                msgBuilder.Append($" - {paramRequiredCount + paramDefaultCount}");
            }

            throw new JsonRpcParamException(msgBuilder.ToString());
        }

        var @params = new object?[info.Length];
        foreach (var (el, i) in json.Value.EnumerateWithIndex())
        {
            if (i >= info.Length)
            {
                throw new JsonRpcParamException($"Invalid param count - Expected: {info.Length}");
            }

            @params[i] = ParseSingleParam(el, info[i], i);
        }

        if (@params.Length != info.Length)
        {
            throw new JsonRpcParamException($"Invalid param count - Expected: {info.Length}");
        }

        return @params;
    }

    private static object? GetTaskResult(Task task) =>
        task.GetType().GetProperty(nameof(Task<object>.Result))?.GetValue(task);
}
