using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.RecordRedirectHit;

// ADR-024 §15 (post-Faz-1a fix): no endpoint of its own - ResolveRouteEndpoint sends this after
// resolution concludes Redirect against a real Redirect row, deliberately outside the resolution
// query itself (AGENTS.md §13: a query must not write), mirroring RecordNotFoundPathCommand.
public sealed record RecordRedirectHitCommand(Guid RedirectId) : IRequest<Result>;
