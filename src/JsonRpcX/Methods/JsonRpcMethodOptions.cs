using System.Reflection;
using System.Text.Json;
using JsonRpcX.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace JsonRpcX.Methods;

/// <summary>
/// Options for configuring JSON RPC method functionality.
/// </summary>
public class JsonRpcMethodOptions
{
    /// <summary>
    /// Default method naming policy.
    /// </summary>
    public JsonNamingPolicy? NamingPolicy { get; set; }

    /// <summary>
    /// Custom method name resolver.<br />
    /// <br />
    /// This overrides the default naming policy.
    /// </summary>
    public Func<MethodInfo, JsonRpcMethodAttribute, string>? NameResolver { get; set; }

    /// <summary>
    /// Custom method authorization resolver.
    /// </summary>
    public Func<MethodInfo, IEnumerable<IAuthorizeData>>? AuthorizationResolver { get; set; }
}
