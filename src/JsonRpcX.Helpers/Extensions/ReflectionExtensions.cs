using System.Reflection;

namespace JsonRpcX.Helpers.Extensions;

public static class ReflectionExtensions
{
    private static readonly NullabilityInfoContext NullabilityCtx = new();

    public static bool IsNullable(this ParameterInfo info) =>
        Nullable.GetUnderlyingType(info.ParameterType) != null
        || NullabilityCtx.Create(info).WriteState == NullabilityState.Nullable;
}
