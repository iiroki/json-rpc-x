using System.Text.Json;
using JsonRpcX.Methods;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Core.Schema;

// TODO: Rename and re-implement this!
internal class JsonRpcSchemaEndpointFactory
{
    public RequestDelegate Create() =>
        async (ctx) =>
        {
            var services = ctx.RequestServices;
            var container = services.GetRequiredService<IJsonRpcMethodContainer>();
            var jsonOptions = services.GetService<JsonSerializerOptions>();

            // TODO: Think about what's needed in the schema?
            var schema = new { Methods = container.Methods.Keys.Order() };

            var bytes = JsonSerializer.SerializeToUtf8Bytes(schema, jsonOptions);

            ctx.Response.ContentType = "appilication/json";
            await ctx.Response.BodyWriter.WriteAsync(bytes);
        };
}
