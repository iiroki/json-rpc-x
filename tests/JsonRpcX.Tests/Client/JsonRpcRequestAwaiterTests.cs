using JsonRpcX.Client;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Tests.Client;

public class JsonRpcRequestAwaiterTests
{
    private readonly JsonRpcRequestAwaiter _awaiter = new();

    [Fact]
    public async Task WaitForResponse_Valid_Ok()
    {
        // Arrange
        var clientId = Guid.NewGuid().ToString();
        var requestId = Guid.NewGuid().ToString();
        var expected = new JsonRpcResponseSuccess { Id = requestId }.ToResponse();

        // Act
        var task = _awaiter.WaitForResponseAsync(clientId, requestId);
        _awaiter.SetResponse(clientId, expected);
        var actual = await task;

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(0, _awaiter.Count);
    }

    [Fact]
    public async Task WaitForResponse_Timeout_Exception()
    {
        // Arrange
        var clientId = Guid.NewGuid().ToString();
        var requestId = Guid.NewGuid().ToString();

        // Act
        var task = _awaiter.WaitForResponseAsync(clientId, requestId, TimeSpan.FromMilliseconds(10));

        // Assert
        await Assert.ThrowsAsync<JsonRpcTimeoutException>(async () => await task);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void SetResponse_InvalidResponseId(string? responseId)
    {
        // Arrange
        var response = new JsonRpcResponseSuccess { Id = responseId }.ToResponse();

        // Act
        _awaiter.SetResponse("", response);
    }

    [Fact]
    public async Task SetResponse_InvalidClientId()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var response = new JsonRpcResponseSuccess { Id = requestId }.ToResponse();

        // Act
        var task = _awaiter.WaitForResponseAsync(Guid.NewGuid().ToString(), requestId, TimeSpan.FromSeconds(1));
        _awaiter.SetResponse(Guid.NewGuid().ToString(), response);

        // Assert
        await Assert.ThrowsAsync<JsonRpcTimeoutException>(async () => await task);
        Assert.Equal(0, _awaiter.Count); // The request should also be removed
    }
}
