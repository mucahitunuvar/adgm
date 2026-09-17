namespace GenclikMerkezi.Modules.Candidate.Features.AddCandidateLanguage;

public sealed record AddCandidateLanguageRequest(Guid LanguageId, Guid LanguageLevelId, bool IsNativeLanguage);
