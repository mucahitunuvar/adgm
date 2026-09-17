namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;

public sealed record CandidateLanguageResponse(Guid Id, Guid LanguageId, Guid LanguageLevelId, bool IsNativeLanguage);
