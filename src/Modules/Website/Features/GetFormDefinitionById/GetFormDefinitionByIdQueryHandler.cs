using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetFormDefinitionById;

public sealed class GetFormDefinitionByIdQueryHandler(IFormDefinitionRepository formDefinitionRepository)
    : IRequestHandler<GetFormDefinitionByIdQuery, Result<FormDefinitionDetailResponse>>
{
    public async Task<Result<FormDefinitionDetailResponse>> Handle(GetFormDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var form = await formDefinitionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (form is null)
        {
            return Result.Failure<FormDefinitionDetailResponse>(
                Error.NotFound("FormDefinition.NotFound", $"Form '{request.Id}' could not be found."));
        }

        var translations = form.Translations
            .Select(t => new FormDefinitionTranslationResponse(t.LanguageCode.Value, t.Title, t.Description, t.SuccessMessage, t.SubmitButtonLabel))
            .ToList();

        var explicitConsents = form.ExplicitConsents
            .Select(c => new FormExplicitConsentResponse(c.LegalDocumentKey.Value, c.IsRequired))
            .ToList();

        var fields = form.Fields
            .OrderBy(f => f.SortOrder)
            .Select(f => new FormFieldResponse(
                f.Key,
                f.Type.ToString(),
                f.IsRequired,
                f.SortOrder,
                f.MinLength,
                f.MaxLength,
                f.Options.Select(o => new FormFieldOptionResponse(
                    o.Key, o.Translations.Select(t => new FormFieldOptionTranslationResponse(t.LanguageCode.Value, t.Label)).ToList())).ToList(),
                f.DateMin,
                f.DateMax,
                f.AllowedFileTypes.Select(t => t.ToString()).ToList(),
                f.MaxSizeMb,
                f.Translations.Select(t => new FormFieldTranslationResponse(t.LanguageCode.Value, t.Label, t.Placeholder, t.HelpText)).ToList()))
            .ToList();

        var response = new FormDefinitionDetailResponse(
            form.Id, form.Key.Value, form.IsActive, form.RetentionDays, form.NotificationEmails, form.PrivacyNoticeKey.Value, explicitConsents,
            form.DefinitionVersion, form.RowVersion, form.CreatedAtUtc, translations, fields);

        return Result.Success(response);
    }
}
