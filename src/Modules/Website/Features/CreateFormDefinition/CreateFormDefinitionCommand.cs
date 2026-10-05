using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateFormDefinition;

public sealed record CreateFormDefinitionCommand(
    string? Key,
    int? RetentionDays,
    IReadOnlyList<string> NotificationEmails,
    string? PrivacyNoticeKey,
    IReadOnlyList<CreateFormDefinitionExplicitConsentInput> ExplicitConsents,
    string? DefaultLanguageTitle,
    string? DefaultLanguageDescription,
    string? DefaultLanguageSuccessMessage,
    string? DefaultLanguageSubmitButtonLabel) : IRequest<Result<CreateFormDefinitionResponse>>;
