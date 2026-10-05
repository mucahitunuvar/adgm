using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.SubmitFormSubmission;

public sealed record SubmitFormSubmissionCommand(
    string FormKey,
    string? SubmissionToken,
    string? TurnstileToken,
    string? Website,
    string? Lang,
    int? AcceptedPrivacyNoticeVersion,
    IReadOnlyList<SubmitFormSubmissionExplicitConsentInput> ExplicitConsents,
    Guid? ContentItemId,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Answers,
    IReadOnlyList<SubmitFormSubmissionFileInput> Files,
    string? RemoteIpAddress) : IRequest<Result<SubmitFormSubmissionResponse>>;
