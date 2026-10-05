using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateFormDefinitionTranslation;

public sealed record UpdateFormDefinitionTranslationCommand(
    Guid Id, string LanguageCode, byte[] RowVersion, string? Title, string? Description, string? SuccessMessage, string? SubmitButtonLabel)
    : IRequest<Result>;
