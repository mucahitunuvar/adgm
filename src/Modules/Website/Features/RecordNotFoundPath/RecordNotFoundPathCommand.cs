using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.RecordNotFoundPath;

// ADR-024 §15 (Faz 1a Görev 5): no endpoint of its own - Görev 6's public route-resolution endpoint
// sends this after resolution concludes NotFound, deliberately outside the resolution query itself
// (AGENTS.md §13: a query must not write).
public sealed record RecordNotFoundPathCommand(string? LanguageCode, string? Path) : IRequest<Result>;
