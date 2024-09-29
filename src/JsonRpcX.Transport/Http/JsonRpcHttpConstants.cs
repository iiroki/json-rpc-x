using System.Collections.Immutable;
using System.Net;
using JsonRpcX.Domain.Constants;

namespace JsonRpcX.Transport.Http;

public static class JsonRpcHttpConstants
{
    public const string ContentTypeJsonRpc = "application/json-rpc";
    public const string ContentTypeJson = "application/json";
    public const string ContentTypeJsonRequest = "application/jsonrequest";

    /// <summary>
    /// HTTP status codes according to
    /// <see href="https://www.jsonrpc.org/historical/json-rpc-over-http.html#errors">JSON-RPC over HTTP</see>.<br />
    /// <br />
    /// The specification says that "Invalid params" should result in HTTP 500,
    /// but we'll use HTTP 400 as it describes the situation better.
    /// </summary>
    public static readonly ImmutableDictionary<int, int> StatusBindings = new Dictionary<int, int>
    {
        { (int)JsonRpcErrorCode.ParseError, (int)HttpStatusCode.BadRequest },
        { (int)JsonRpcErrorCode.InvalidRequest, (int)HttpStatusCode.BadRequest },
        { (int)JsonRpcErrorCode.InvalidParams, (int)HttpStatusCode.BadRequest },
        { (int)JsonRpcErrorCode.MethodNotFound, (int)HttpStatusCode.NotFound },
        { (int)JsonRpcErrorCode.InternalError, (int)HttpStatusCode.InternalServerError },
    }.ToImmutableDictionary();
}
