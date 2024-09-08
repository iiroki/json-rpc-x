using System.Collections.Immutable;
using System.Net;
using JsonRpcX.Domain.Constants;
using JsonRpcX.Domain.Interfaces;
using JsonRpcX.Domain.Models;
using JsonRpcX.Transport.Constants;
using JsonRpcX.Transport.Interfaces;
using JsonRpcX.Transport.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Transport.Http;

internal class JsonRpcHttpTransport : IJsonRpcTransport
{
    public string Type { get; } = JsonRpcTransportType.Http;

    /// <summary>
    /// HTTP status codes according to
    /// <see href="https://www.jsonrpc.org/historical/json-rpc-over-http.html#errors">JSON-RPC over HTTP</see>.<br />
    /// <br />
    /// The specification says that "Invalid params" should result in HTTP 500,
    /// but we'll use HTTP 400 as it describes the situation better.
    /// </summary>
    private static readonly ImmutableDictionary<int, int> StatusCodes = new Dictionary<int, int>
    {
        { (int)JsonRpcErrorCode.ParseError, (int)HttpStatusCode.BadRequest },
        { (int)JsonRpcErrorCode.InvalidRequest, (int)HttpStatusCode.BadRequest },
        { (int)JsonRpcErrorCode.InvalidParams, (int)HttpStatusCode.BadRequest },
        { (int)JsonRpcErrorCode.MethodNotFound, (int)HttpStatusCode.NotFound },
        { (int)JsonRpcErrorCode.InternalError, (int)HttpStatusCode.InternalServerError },
    }.ToImmutableDictionary();

    public RequestDelegate Delegate { get; } =
        async httpCtx =>
        {
            // Request a processor that does not serialize the response,
            // since we want to extract the status code from it.
            var processor = httpCtx.RequestServices.GetRequiredService<IJsonRpcProcessor<byte[], JsonRpcResponse>>();
            var serializer = httpCtx.RequestServices.GetRequiredService<IJsonRpcResponseSerializer<byte[]>>();

            var contentLength = (int)(httpCtx.Request.ContentLength ?? 0);
            if (contentLength == 0)
            {
                httpCtx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            // Read the request content
            var buffer = new byte[contentLength];
            await httpCtx.Request.Body.ReadAsync(buffer.AsMemory(0, contentLength), httpCtx.RequestAborted);

            // Process the request
            var ctx = new JsonRpcContext { Http = httpCtx };
            var response = await processor.ProcessAsync(buffer, ctx);

            if (response != null)
            {
                if (response.IsSuccess)
                {
                    httpCtx.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    httpCtx.Response.StatusCode = StatusCodes.GetValueOrDefault(
                        response.Error.Error.Code,
                        (int)HttpStatusCode.InternalServerError
                    );
                }

                httpCtx.Response.ContentType = "application/json";
                var serialized = serializer.Serialize(response);
                await httpCtx.Response.BodyWriter.WriteAsync(serialized);
            }
            else
            {
                httpCtx.Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
        };
}
