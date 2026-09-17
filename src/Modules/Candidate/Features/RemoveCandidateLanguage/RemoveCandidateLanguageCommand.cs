using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.RemoveCandidateLanguage;

public sealed record RemoveCandidateLanguageCommand(Guid CandidateCvId, Guid CandidateLanguageId) : IRequest<Result>;
