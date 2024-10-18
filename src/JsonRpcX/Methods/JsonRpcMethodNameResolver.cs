using System.Reflection;
using System.Text.Json;
using JsonRpcX.Attributes;

namespace JsonRpcX.Methods;

internal static class JsonRpcMethodNameResolver
{
    public static string GetName(MethodInfo method, JsonRpcMethodAttribute attr, JsonNamingPolicy? namingPolicy)
    {
        // Name from the attribute overrides everything else
        if (!string.IsNullOrEmpty(attr.Name))
        {
            return attr.Name;
        }

        var name = method.Name;

        // Drop the async suffix from the method name, if one exists.
        const string asyncSuffix = "Async";
        if (name.EndsWith(asyncSuffix, StringComparison.OrdinalIgnoreCase))
        {
            name = name[..^asyncSuffix.Length];
        }

        // Use a naming policy if one is defined
        if (namingPolicy != null)
        {
            return namingPolicy.ConvertName(name);
        }

        return name;
    }
}
