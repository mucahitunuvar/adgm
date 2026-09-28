using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ResolveRoute;

public sealed record ResolveRouteQuery(string? Path) : IRequest<Result<RouteResolutionResponse>>;
