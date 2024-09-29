using System.Text.Json;

namespace JsonRpcX.Helpers.Constants;

public static class JsonConstants
{
    public static readonly JsonElement Null = JsonSerializer.SerializeToElement((object?)null);

    public static readonly JsonElement Undefined = default;
}
