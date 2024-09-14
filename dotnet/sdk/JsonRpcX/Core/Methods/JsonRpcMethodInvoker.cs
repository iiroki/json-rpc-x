using System.Reflection;
using System.Text;
using System.Text.Json;
using JsonRpcX.Domain.Exceptions;
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
        catch (ArgumentException ex)
        {
            throw new JsonRpcException("Received invalid params", ex);
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

        object?[]? parsedParams;
        if (methodParams.Length == 1 && json.HasValue && json.Value.IsArray())
        {
            parsedParams = [ParseParam(json, methodParams.First(), 0)];
        }
        else if (methodParams.Length > 0)
        {
            parsedParams =
                json.HasValue && json.Value.IsArray()
                    ? ParseParams(json, methodParams)
                    : [ParseParam(json, methodParams.First(), 0)];
        }
        else
        {
            parsedParams = [];
        }

        return @hasCt ? [.. parsedParams, ct] : parsedParams;
    }

    private object? ParseParam(JsonElement? json, ParameterInfo info, int index)
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

        var parsed = 0;
        foreach (var (el, i) in json.Value.EnumerateWithIndex())
        {
            if (i >= info.Length)
            {
                throw new JsonRpcParamException($"Invalid param count - Expected: {info.Length}");
            }

            @params[i] = ParseParam(el, info[i], i);
            ++parsed;
        }

        if (parsed != info.Length)
        {
            throw new JsonRpcParamException($"Invalid param count - Expected: {info.Length}");
        }

        return @params;
    }

    private static object? GetTaskResult(Task task) =>
        task.GetType().GetProperty(nameof(Task<object>.Result))?.GetValue(task);
}
