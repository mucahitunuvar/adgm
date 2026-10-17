using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.ExceptionHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // BadHttpRequestException is minimal API's own model-binding failure (e.g. a malformed
        // "Guid? id" query value) - a malformed REQUEST, not a server fault. Confirmed live: an
        // endpoint binding a nullable value-type query parameter directly (rather than through
        // OptionalQueryParameter) turns ?id=not-a-guid into an unhandled exception here; without this
        // branch every such case fell through to the generic 500 below. This is a safety net for any
        // endpoint OptionalQueryParameter wasn't used on (new or missed) - it is not a substitute for
        // using OptionalQueryParameter where "empty value = filter not applied" is required, since this
        // branch cannot distinguish that nuance (the framework already failed to bind by the time this
        // runs) and always returns 400 for both an empty and a malformed value alike.
        if (exception is BadHttpRequestException badRequestException)
        {
            logger.LogWarning(badRequestException, "Bad request while processing {Path}", httpContext.Request.Path);

            httpContext.Response.StatusCode = badRequestException.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    title = "Bad Request",
                    detail = badRequestException.Message,
                    status = badRequestException.StatusCode,
                },
                cancellationToken);

            return true;
        }

        logger.LogError(exception, "Unhandled exception occurred while processing {Path}", httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                title = "An unexpected error occurred.",
                status = StatusCodes.Status500InternalServerError,
            },
            cancellationToken);

        return true;
    }
}
