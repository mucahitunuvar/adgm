using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateFormDefinition;

public sealed record UpdateFormDefinitionCommand(
    Guid Id,
    byte[] RowVersion,
    int? RetentionDays,
    IReadOnlyList<string> NotificationEmails,
    string? PrivacyNoticeKey,
    IReadOnlyList<UpdateFormDefinitionExplicitConsentInput> ExplicitConsents) : IRequest<Result>;
