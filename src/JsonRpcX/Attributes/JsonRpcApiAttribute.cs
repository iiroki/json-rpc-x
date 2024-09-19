namespace JsonRpcX.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class JsonRpcApiAttribute(string? id = null) : Attribute
{
    public string? Id { get; init; } = id;
}
