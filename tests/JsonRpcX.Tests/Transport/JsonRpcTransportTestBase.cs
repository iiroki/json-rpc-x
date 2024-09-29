using JsonRpcX.Attributes;
using JsonRpcX.Methods;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Tests.Transport;

public abstract class JsonRpcTransportTestBase : IDisposable
{
    protected const string TestUrl = "http://localhost:65431";
    protected const string TestRoute = "/json-rpc-test";
    protected const string TestEndpoint = TestUrl + TestRoute;

    protected WebApplication? App;

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        var task = App?.DisposeAsync();
        if (task.HasValue)
        {
            SpinWait.SpinUntil(() => task.Value.IsCompleted, TimeSpan.FromSeconds(5));
        }
    }

    protected static WebApplication CreateTestApp(params Type[] handlers)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(TestUrl);
        builder.Services.AddJsonRpc().AddJsonRpcMethodHandler<TestJsonRpcApi>();

        foreach (var h in handlers)
        {
            builder.Services.AddJsonRpcMethodHandler(h);
        }

        builder.Logging.AddFilter(null, LogLevel.None);
        return builder.Build();
    }

    protected class TestJsonRpcApi : IJsonRpcMethodHandler
    {
        [JsonRpcMethod]
        public static string Method() => "Hello";

        [JsonRpcMethod]
        public static void Error() => throw new InvalidOperationException();
    }
}
