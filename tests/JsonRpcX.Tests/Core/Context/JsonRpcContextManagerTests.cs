using System.Security.Claims;
using JsonRpcX.Core.Context;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Tests.Core.Context;

public class JsonRpcContextManagerTests
{
    private readonly JsonRpcContextManager _manager = new();

    [Fact]
    public void SetContext_Ok()
    {
        // Arrange
        var ctx = new JsonRpcContext { Transport = "TEST", User = new ClaimsPrincipal() };

        // Act
        _manager.SetContext(ctx);

        // Assert
        Assert.Equal(ctx, _manager.Context);
    }

    [Fact]
    public void Context_NotInit_Exception()
    {
        // Assert
        Assert.Throws<InvalidOperationException>(() => _manager.Context);
    }
}
