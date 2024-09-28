using System.Net;
using JsonRpcX.Domain.Core;
using JsonRpcX.Domain.Models;
using JsonRpcX.Transport.Constants;
using JsonRpcX.Transport.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Transport.Http;

internal class JsonRpcHttpTransport
{
    public RequestDelegate Delegate { get; } =
        async httpCtx =>
        {
            var ct = httpCtx.RequestAborted;

            if (!JsonRpcHttpHelper.HasValidContentType(httpCtx.Request))
            {
                httpCtx.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                return;
            }

            if (!JsonRpcHttpHelper.HasValidAccept(httpCtx.Request))
            {
                httpCtx.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                return;
            }

            if (!httpCtx.Request.ContentLength.HasValue)
            {
                httpCtx.Response.StatusCode = (int)HttpStatusCode.LengthRequired;
                return;
            }

            // Read the request content
            var contentLength = (int)httpCtx.Request.ContentLength;
            var buffer = new byte[contentLength];
            await httpCtx.Request.Body.ReadAsync(buffer.AsMemory(0, contentLength), ct);

            // Process the request
            // Request a processor that does not serialize the response,
            // since we want to extract the status code from it.
            var processor = httpCtx.RequestServices.GetRequiredService<IJsonRpcProcessor<byte[], JsonRpcResponse>>();
            var serializer = httpCtx.RequestServices.GetRequiredService<IJsonRpcResponseSerializer<byte[]>>();

            var ctx = new JsonRpcContext { Transport = JsonRpcTransportType.Http, User = httpCtx.User };
            var response = await processor.ProcessAsync(buffer, ctx);

            if (response != null)
            {
                if (response.IsSuccess)
                {
                    httpCtx.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    httpCtx.Response.StatusCode = JsonRpcHttpHelper.GetStatus(response.Error.Error.Code);
                }

                httpCtx.Response.ContentType = JsonRpcHttpConstants.ContentTypeJsonRpc;
                var serialized = serializer.Serialize(response);
                await httpCtx.Response.BodyWriter.WriteAsync(serialized);
            }
            else
            {
                httpCtx.Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
        };
}
