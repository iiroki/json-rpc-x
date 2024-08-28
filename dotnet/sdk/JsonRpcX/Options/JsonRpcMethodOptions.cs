using System.Text.Json;

namespace JsonRpcX.Options;

/// <summary>
/// Options for configuring JSON RPC method functionality.
/// </summary>
public class JsonRpcMethodOptions
{
    public JsonNamingPolicy? MethodNamingPolicy { get; set; }

    public Func<string, string>? MethodNameTransformer { get; set; }
}
