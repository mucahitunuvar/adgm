using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.AddCandidateLanguage;

public sealed record AddCandidateLanguageCommand(
    Guid CandidateCvId, Guid LanguageId, Guid LanguageLevelId, bool IsNativeLanguage) : IRequest<Result<Guid>>;
