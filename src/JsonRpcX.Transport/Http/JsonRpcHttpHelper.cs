using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace JsonRpcX.Transport.Http;

public static class JsonRpcHttpHelper
{
    private const string Utf8Charset = "utf-8";

    private static readonly MediaTypeHeaderValue DefaultContentType = new(JsonRpcHttpConstants.ContentTypeJsonRpc);

    private static readonly ImmutableList<MediaTypeHeaderValue> ValidContentTypes =
    [
        CreateUtf8ContentType(JsonRpcHttpConstants.ContentTypeJsonRpc),
        CreateUtf8ContentType(JsonRpcHttpConstants.ContentTypeJson),
        CreateUtf8ContentType(JsonRpcHttpConstants.ContentTypeJsonRequest),
    ];

    public static bool HasValidContentType(HttpRequest req) => IsValidContentType(req.GetTypedHeaders().ContentType);

    public static bool IsValidContentType(MediaTypeHeaderValue? contentType)
    {
        if (contentType == null)
        {
            return false;
        }

        if (!HasValidCharset(contentType))
        {
            return false;
        }

        return HasValidMediaType(contentType);
    }

    public static (bool, MediaTypeHeaderValue?) HasValidAccept(HttpRequest req)
    {
        var accept = req.GetTypedHeaders().Accept;
        if (accept.Count == 0)
        {
            return (true, DefaultContentType);
        }

        foreach (var a in accept)
        {
            if (!HasValidCharset(a))
            {
                continue;
            }

            if (a.MatchesAllTypes)
            {
                return (true, DefaultContentType);
            }

            if (TryGetValidContentType(a, out var valid))
            {
                return (true, valid);
            }
        }

        return (false, null);
    }

    public static int GetStatus(int code, int? @default = null) =>
        JsonRpcHttpConstants.StatusBindings.GetValueOrDefault(
            code,
            @default ?? (int)HttpStatusCode.InternalServerError
        );

    private static bool TryGetValidContentType(
        MediaTypeHeaderValue contentType,
        [MaybeNullWhen(false)] out MediaTypeHeaderValue validContentType
    )
    {
        var value = ValidContentTypes.FirstOrDefault(c => c.MediaType == contentType.MediaType);
        validContentType = value;
        return validContentType != null;
    }

    private static bool HasValidMediaType(MediaTypeHeaderValue contentType) =>
        contentType.MediaType.HasValue && TryGetValidContentType(contentType, out _);

    private static bool HasValidCharset(MediaTypeHeaderValue contentType) =>
        !contentType.Charset.HasValue
        || contentType.Charset.Value.Equals(Utf8Charset, StringComparison.CurrentCultureIgnoreCase);

    private static MediaTypeHeaderValue CreateUtf8ContentType(string contentType) =>
        new(contentType) { Charset = Utf8Charset };
}
