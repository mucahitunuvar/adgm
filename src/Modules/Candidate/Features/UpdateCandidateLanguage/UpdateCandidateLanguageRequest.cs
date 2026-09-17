namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateLanguage;

public sealed record UpdateCandidateLanguageRequest(Guid LanguageId, Guid LanguageLevelId, bool IsNativeLanguage);
