using System.Text.Json;

namespace JsonRpcX.Options;

/// <summary>
/// Options for configuring JSON RPC method functionality.
/// </summary>
public class JsonRpcMethodOptions
{
    public JsonNamingPolicy? NamingPolicy { get; set; }

    public Func<string, string>? NameTransformer { get; set; }
}
