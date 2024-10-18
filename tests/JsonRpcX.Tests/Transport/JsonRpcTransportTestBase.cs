using System.Text.Json;
using JsonRpcX.Attributes;
using JsonRpcX.Controllers;
using JsonRpcX.Transport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Tests.Transport;

public abstract class JsonRpcTransportTestBase : IDisposable
{
    protected static readonly JsonSerializerOptions JsonOpt = new() { PropertyNameCaseInsensitive = true };

    private WebApplication? _app;

    protected WebApplication TestApp => _app ?? throw new InvalidOperationException("Test app not initialized");

    protected string TestAppUrl => "localhost:" + TestApp.Urls.First().Split(':').Last();

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        DisposeInternal();

        var task = _app?.DisposeAsync();
        if (task.HasValue)
        {
            SpinWait.SpinUntil(() => task.Value.IsCompleted, TimeSpan.FromSeconds(5));
        }
    }

    protected void InitTestApp(Action<IServiceCollection>? serviceFn = null)
    {
        _app = CreateTestApp(serviceFn);
    }

    protected static WebApplication CreateTestApp(Action<IServiceCollection>? serviceFn = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://*:0");

        builder
            .Services.AddSingleton(JsonOpt)
            .AddAuthorization()
            .AddJsonRpc()
            .AddJsonRpcWebSocket()
            .AddJsonRpcController<TestJsonRpcApi>();

        serviceFn?.Invoke(builder.Services);

        builder.Logging.AddFilter(null, LogLevel.None);
        return builder.Build();
    }

    protected virtual void DisposeInternal()
    {
        // NOP
    }

    protected class TestJsonRpcApi : IJsonRpcController
    {
        [JsonRpcMethod]
        public static string Method() => "Hello";

        [JsonRpcMethod]
        public static void Error() => throw new InvalidOperationException();

        [JsonRpcMethod]
        public static int Params(int a, int b) => a + b;

        [JsonRpcMethod]
        public static async Task Async(int ms) => await Task.Delay(ms);
    }
}
