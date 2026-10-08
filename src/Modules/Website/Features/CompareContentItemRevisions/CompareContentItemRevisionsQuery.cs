using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CompareContentItemRevisions;

// To is either a revision number ("5") or the literal "current" (ADR-024 §4: "revizyon ↔ mevcut
// içerik, to=current").
public sealed record CompareContentItemRevisionsQuery(Guid ContentItemId, int From, string? To, string? LanguageCode)
    : IRequest<Result<CompareContentItemRevisionsResponse>>;
