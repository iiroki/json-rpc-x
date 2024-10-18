using System.Net;
using JsonRpcX.Domain;
using JsonRpcX.Domain.Models;
using JsonRpcX.Transport.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace JsonRpcX.Transport.Http;

internal class JsonRpcHttpHandler
{
    public RequestDelegate Delegate { get; } =
        async httpCtx =>
        {
            var ct = httpCtx.RequestAborted;
            var validation = ValidateRequest(httpCtx.Request);
            if (validation.ErrorStatus.HasValue)
            {
                httpCtx.Response.StatusCode = (int)validation.ErrorStatus;
                return;
            }

            // Read the request content
            var contentLength = (int)(httpCtx.Request.ContentLength ?? 0);
            var buffer = new byte[contentLength];
            await httpCtx.Request.Body.ReadAsync(buffer.AsMemory(0, contentLength), ct);

            // Initialize services
            var processor = httpCtx.RequestServices.GetRequiredService<IJsonRpcProcessor<byte[], JsonRpcResponse>>();
            var serializer = httpCtx.RequestServices.GetRequiredService<IJsonRpcOutSerializer<byte[]>>();
            var ctx = new JsonRpcContext { Transport = JsonRpcTransportType.Http, User = httpCtx.User };

            // Process the request
            var response = await processor.ProcessAsync(buffer, ctx);
            if (response != null)
            {
                if (response.IsSuccess)
                {
                    httpCtx.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    httpCtx.Response.StatusCode = JsonRpcHttpHelper.GetStatus(response.Error?.Code ?? 0);
                }

                var serialized = serializer.Serialize(response);

                httpCtx.Response.ContentType = validation.ResponseContentType?.ToString();
                await httpCtx.Response.BodyWriter.WriteAsync(serialized);
            }
            else
            {
                httpCtx.Response.ContentType = null;
                httpCtx.Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
        };

    private static JsonRpcHttpRequest ValidateRequest(HttpRequest req)
    {
        if (!JsonRpcHttpHelper.HasValidContentType(req))
        {
            return new(HttpStatusCode.UnsupportedMediaType, null);
        }

        var (isValidAccept, resContentType) = JsonRpcHttpHelper.HasValidAccept(req);
        if (!isValidAccept)
        {
            return new(HttpStatusCode.NotAcceptable, resContentType);
        }

        if (!req.ContentLength.HasValue)
        {
            return new(HttpStatusCode.LengthRequired, resContentType);
        }

        return new(null, resContentType);
    }

    private record JsonRpcHttpRequest(HttpStatusCode? ErrorStatus, MediaTypeHeaderValue? ResponseContentType);
}
