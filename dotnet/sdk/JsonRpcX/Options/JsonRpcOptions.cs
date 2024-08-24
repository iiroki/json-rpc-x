using System.Text.Json;

namespace JsonRpcX.Options;

/// <summary>
/// Options for configuring JSON RPC functionality.
/// </summary>
public class JsonRpcOptions
{
    public JsonNamingPolicy? MethodNamingPolicy { get; set; }

    public Func<string, string>? MethodNameTransformer { get; set; }
}
