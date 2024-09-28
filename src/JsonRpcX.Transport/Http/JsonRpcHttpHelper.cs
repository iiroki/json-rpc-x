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

    public static bool HasValidContentType(HttpRequest req)
    {
        var type = req.GetTypedHeaders().ContentType?.MediaType;
        return type.HasValue && IsValidContentType(type.Value.Value);
    }

    public static bool IsValidContentType(string? contentType) =>
        !string.IsNullOrEmpty(contentType) && ValidContentTypes.Contains(contentType);

    public static bool HasValidAccept(HttpRequest req)
    {
        var accept = req.GetTypedHeaders().Accept;
        if (accept.Count == 0)
        {
            return true;
        }

        foreach (var a in accept)
        {
            if (a.MatchesAllTypes)
            {
                return true;
            }

            if (a.MediaType.HasValue && ValidContentTypes.Contains(a.MediaType.Value))
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
