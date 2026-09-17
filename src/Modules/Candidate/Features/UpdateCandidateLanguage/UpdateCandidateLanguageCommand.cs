using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateLanguage;

public sealed record UpdateCandidateLanguageCommand(
    Guid CandidateCvId, Guid CandidateLanguageId, Guid LanguageId, Guid LanguageLevelId, bool IsNativeLanguage) : IRequest<Result>;
