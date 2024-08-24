namespace JsonRpcX.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class JsonRpcMethodAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
}
