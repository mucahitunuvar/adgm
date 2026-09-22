using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Matching.Features.GetSuggestionsForPersonnelNeed;

public sealed record GetSuggestionsForPersonnelNeedQuery(Guid PersonnelNeedId)
    : IRequest<Result<IReadOnlyList<CandidateSuggestionResponse>>>;
