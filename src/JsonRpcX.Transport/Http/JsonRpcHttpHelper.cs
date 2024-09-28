using System.Net;
using Microsoft.AspNetCore.Http;

namespace JsonRpcX.Transport.Http;

public static class JsonRpcHttpHelper
{
    private static readonly HashSet<string> ValidContentTypes =
    [
        JsonRpcHttpConstants.ContentTypeJsonRpc,
        JsonRpcHttpConstants.ContentTypeJson,
        JsonRpcHttpConstants.ContentTypeJsonRequest,
    ];

    public static bool HasValidContentType(HttpRequest req) => IsValidContentType(req.ContentType);

    public static bool IsValidContentType(string? contentType) =>
        !string.IsNullOrEmpty(contentType) && ValidContentTypes.Contains(contentType);

    public static bool HasValidAccept(HttpRequest req)
    {
        foreach (var accept in req.GetTypedHeaders().Accept)
        {
            if (accept.MatchesAllTypes)
            {
                return true;
            }

            if (accept.MediaType.HasValue && ValidContentTypes.Contains(accept.MediaType.Value))
            {
                return true;
            }
        }
        return false;
    }

    public static int GetStatus(int code, int? @default = null) =>
        JsonRpcHttpConstants.StatusBindings.GetValueOrDefault(
            code,
            @default ?? (int)HttpStatusCode.InternalServerError
        );
}
