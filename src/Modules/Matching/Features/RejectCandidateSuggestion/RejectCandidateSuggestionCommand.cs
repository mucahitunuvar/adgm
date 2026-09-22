using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Matching.Features.RejectCandidateSuggestion;

public sealed record RejectCandidateSuggestionCommand(Guid CandidateSuggestionId) : IRequest<Result>;
