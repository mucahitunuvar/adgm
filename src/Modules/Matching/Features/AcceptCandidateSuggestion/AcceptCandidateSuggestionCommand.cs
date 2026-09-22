using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Matching.Features.AcceptCandidateSuggestion;

public sealed record AcceptCandidateSuggestionCommand(Guid CandidateSuggestionId) : IRequest<Result>;
