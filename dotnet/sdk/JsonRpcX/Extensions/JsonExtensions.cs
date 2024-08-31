using System.Text.Json;

namespace JsonRpcX.Extensions;

public static class JsonExtensions
{
    public static bool IsNull(this JsonElement json) => json.ValueKind == JsonValueKind.Null;

    public static bool IsArray(this JsonElement json) => json.ValueKind == JsonValueKind.Array;

    public static bool IsValueKindOneOf(this JsonElement json, params JsonValueKind[] kinds) =>
        kinds.Contains(json.ValueKind);

    public static IEnumerable<(JsonElement, int)> EnumerateWithIndex(this JsonElement json) =>
        json.EnumerateArray().Select((el, i) => (el, i));
}
