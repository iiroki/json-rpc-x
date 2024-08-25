using System.Reflection;
using System.Text;
using System.Text.Json;
using JsonRpcX.Exceptions;
using JsonRpcX.Extensions;
using JsonRpcX.Methods;

namespace JsonRpcX.Core.Methods;

internal class JsonRpcMethodInvoker(IJsonRpcMethodInvocation invocation, JsonSerializerOptions? jsonOptions)
{
    private readonly IJsonRpcMethodHandler _handler = invocation.Handler;
    private readonly MethodInfo _method = invocation.Method;
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
            return _method.Invoke(_handler, @params);
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex;
        }
    }

    private object?[]? GetParameters(JsonElement? json, CancellationToken ct)
    {
        var methodParams = _method.GetParameters();
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
                    : [ParseSingleParam(json, methodParams.First())];
        }
        else
        {
            parsedParams = [];
        }

        return hasCt ? [.. parsedParams, ct] : parsedParams;
    }

    private object? ParseSingleParam(JsonElement? json, ParameterInfo info)
    {
        if (!json.HasValue || json.Value.IsNull())
        {
            return info.HasDefaultValue ? info.DefaultValue : null;
        }

        return json.Value.Deserialize(info.ParameterType, _jsonOptions);
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

            throw new JsonRpcErrorException((int)JsonRpcConstants.ErrorCode.InvalidParams, msgBuilder.ToString());
        }

        List<object?> @params = [];
        foreach (var (el, i) in json.Value.EnumerateWithIndex())
        {
            @params.Add(ParseSingleParam(el, info[i]));
        }

        return [.. @params];
    }

    private static object? GetTaskResult(Task task) =>
        task.GetType().GetProperty(nameof(Task<object>.Result))?.GetValue(task);
}
