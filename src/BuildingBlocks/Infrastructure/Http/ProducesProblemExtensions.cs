using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.Http;

// Declares the standard error response shapes an endpoint can actually produce, so the OpenAPI
// document stops lying about response content (see docs/prompts/FRONTEND-OPENAPI-PRODUCES-PROMPT.md).
// Two families per status code exist because the runtime bodies genuinely differ:
// - The "...Problem" methods document GenclikMerkezi.SharedKernel.Results.Result.ToProblem()'s output
//   (Results.Problem(...) -> "application/problem+json" with title/detail/status), used whenever the
//   endpoint's own handler can return that ErrorType.
// - The bodyless methods document ASP.NET's authentication/authorization/rate-limiter middleware,
//   which (verified against the running API) reject with an empty body and no content-type - there is
//   no ProblemDetails integration configured for those paths.
public static class ProducesProblemExtensions
{
    private const string ProblemContentType = "application/problem+json";

    public static RouteHandlerBuilder ProducesValidationProblem(this RouteHandlerBuilder builder) =>
        builder.Produces<ProblemDetails>(StatusCodes.Status400BadRequest, ProblemContentType);

    public static RouteHandlerBuilder ProducesUnauthorizedProblem(this RouteHandlerBuilder builder) =>
        builder.Produces<ProblemDetails>(StatusCodes.Status401Unauthorized, ProblemContentType);

    public static RouteHandlerBuilder ProducesUnauthenticated(this RouteHandlerBuilder builder) =>
        builder.Produces(StatusCodes.Status401Unauthorized);

    public static RouteHandlerBuilder ProducesForbiddenProblem(this RouteHandlerBuilder builder) =>
        builder.Produces<ProblemDetails>(StatusCodes.Status403Forbidden, ProblemContentType);

    public static RouteHandlerBuilder ProducesRoleForbidden(this RouteHandlerBuilder builder) =>
        builder.Produces(StatusCodes.Status403Forbidden);

    public static RouteHandlerBuilder ProducesNotFoundProblem(this RouteHandlerBuilder builder) =>
        builder.Produces<ProblemDetails>(StatusCodes.Status404NotFound, ProblemContentType);

    public static RouteHandlerBuilder ProducesConflictProblem(this RouteHandlerBuilder builder) =>
        builder.Produces<ProblemDetails>(StatusCodes.Status409Conflict, ProblemContentType);

    public static RouteHandlerBuilder ProducesRateLimited(this RouteHandlerBuilder builder) =>
        builder.Produces(StatusCodes.Status429TooManyRequests);
}
