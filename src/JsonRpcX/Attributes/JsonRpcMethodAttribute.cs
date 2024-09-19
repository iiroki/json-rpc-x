namespace JsonRpcX.Attributes;

/// <summary>
/// Registers JSON RPC method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class JsonRpcMethodAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
}
