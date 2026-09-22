using GenclikMerkezi.Modules.Matching.Features.GetSuggestionsForPersonnelNeed;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Matching.Features.GetMySuggestions;

public sealed record GetMySuggestionsQuery : IRequest<Result<IReadOnlyList<CandidateSuggestionResponse>>>;
