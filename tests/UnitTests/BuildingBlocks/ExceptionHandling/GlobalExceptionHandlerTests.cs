using System.Text.Json;
using GenclikMerkezi.BuildingBlocks.Infrastructure.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenclikMerkezi.UnitTests.BuildingBlocks.ExceptionHandling;

public class GlobalExceptionHandlerTests
{
    private static (DefaultHttpContext Context, MemoryStream Body) CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;
        return (context, body);
    }

    private static async Task<JsonElement> ReadResponseBodyAsync(MemoryStream body)
    {
        body.Position = 0;
        return await JsonSerializer.DeserializeAsync<JsonElement>(body);
    }

    // Confirmed live: minimal API's own Guid?/int?/bool?/DateTime? query-parameter binder throws this
    // exact exception type on a malformed value - a malformed REQUEST, not a server fault, so it must
    // map to 400, not the generic 500 every other unhandled exception gets.
    [Fact]
    public async Task TryHandleAsync_WithBadHttpRequestException_Returns400WithItsOwnMessage()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var (context, body) = CreateHttpContext();
        var exception = new BadHttpRequestException("Failed to bind parameter \"Nullable<Guid> provinceId\" from \"abc\".");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        var json = await ReadResponseBodyAsync(body);
        Assert.Equal("Bad Request", json.GetProperty("title").GetString());
        Assert.Equal(exception.Message, json.GetProperty("detail").GetString());
        Assert.Equal(400, json.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task TryHandleAsync_WithAnyOtherException_Returns500WithGenericMessage()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var (context, body) = CreateHttpContext();
        var exception = new InvalidOperationException("some internal detail that must never reach the client");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var json = await ReadResponseBodyAsync(body);
        Assert.Equal("An unexpected error occurred.", json.GetProperty("title").GetString());
        Assert.False(json.TryGetProperty("detail", out _), "The 500 response must never leak the internal exception message.");
    }
}
