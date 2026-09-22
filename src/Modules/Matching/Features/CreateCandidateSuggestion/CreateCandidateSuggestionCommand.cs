using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Matching.Features.CreateCandidateSuggestion;

public sealed record CreateCandidateSuggestionCommand(Guid PersonnelNeedId, Guid CandidateCvId)
    : IRequest<Result<CreateCandidateSuggestionResponse>>;
