using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.SetFormFields;

// ADR-024 §12.2 (Faz 3 Görev 3): whole-list replace, same shape as SetContentItemGalleryCommandHandler
// - each input DTO is converted into a domain child entity via its own Create(...) factory (which
// enforces the per-Type constraint groups), then the whole list is handed to FormDefinition.SetFields
// (which enforces the whole-form invariants: at most 30 fields, unique keys, at most 3 File fields).
public sealed class SetFormFieldsCommandHandler(
    IFormDefinitionRepository formDefinitionRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SetFormFieldsCommand, Result>
{
    public async Task<Result> Handle(SetFormFieldsCommand request, CancellationToken cancellationToken)
    {
        var form = await formDefinitionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (form is null)
        {
            return Result.Failure(Error.NotFound("FormDefinition.NotFound", $"Form '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(form.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "FormDefinition.ConcurrencyConflict", "The form was changed by someone else. Reload and try again."));
        }

        var fields = new List<FormField>();
        foreach (var fieldInput in request.Fields)
        {
            var fieldResult = BuildField(fieldInput);
            if (fieldResult.IsFailure)
            {
                return fieldResult;
            }

            fields.Add(fieldResult.Value);
        }

        var setResult = form.SetFields(fields, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }

    private static Result<FormField> BuildField(FormFieldInput input)
    {
        if (!Enum.TryParse<FormFieldType>(input.Type, ignoreCase: true, out var type))
        {
            return Result.Failure<FormField>(Error.Validation(
                "FormField.InvalidType", $"'{input.Type}' is not a recognized field type."));
        }

        var options = new List<FormFieldOption>();
        foreach (var optionInput in input.Options)
        {
            var optionTranslationsResult = BuildOptionTranslations(optionInput.Translations);
            if (optionTranslationsResult.IsFailure)
            {
                return Result.Failure<FormField>(optionTranslationsResult.Error);
            }

            var optionResult = FormFieldOption.Create(optionInput.Key, optionTranslationsResult.Value);
            if (optionResult.IsFailure)
            {
                return Result.Failure<FormField>(optionResult.Error);
            }

            options.Add(optionResult.Value);
        }

        var allowedFileTypes = new List<FormFieldAllowedFileType>();
        foreach (var allowedFileType in input.AllowedFileTypes)
        {
            if (!Enum.TryParse<FormFieldAllowedFileType>(allowedFileType, ignoreCase: true, out var parsed))
            {
                return Result.Failure<FormField>(Error.Validation(
                    "FormField.InvalidAllowedFileType", $"'{allowedFileType}' is not a recognized allowed file type."));
            }

            allowedFileTypes.Add(parsed);
        }

        var translationsResult = BuildFieldTranslations(input.Translations);
        if (translationsResult.IsFailure)
        {
            return Result.Failure<FormField>(translationsResult.Error);
        }

        return FormField.Create(
            input.Key, type, input.IsRequired, input.SortOrder, input.MinLength, input.MaxLength, options, input.DateMin, input.DateMax,
            allowedFileTypes, input.MaxSizeMb, translationsResult.Value);
    }

    private static Result<IReadOnlyList<FormFieldOptionTranslation>> BuildOptionTranslations(
        IReadOnlyList<FormFieldOptionTranslationInput> inputs)
    {
        var translations = new List<FormFieldOptionTranslation>();
        foreach (var input in inputs)
        {
            var languageCodeResult = LanguageCode.Create(input.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<FormFieldOptionTranslation>>(languageCodeResult.Error);
            }

            var translationResult = FormFieldOptionTranslation.Create(languageCodeResult.Value, input.Label);
            if (translationResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<FormFieldOptionTranslation>>(translationResult.Error);
            }

            translations.Add(translationResult.Value);
        }

        return Result.Success<IReadOnlyList<FormFieldOptionTranslation>>(translations);
    }

    private static Result<IReadOnlyList<FormFieldTranslation>> BuildFieldTranslations(IReadOnlyList<FormFieldTranslationInput> inputs)
    {
        var translations = new List<FormFieldTranslation>();
        foreach (var input in inputs)
        {
            var languageCodeResult = LanguageCode.Create(input.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<FormFieldTranslation>>(languageCodeResult.Error);
            }

            var translationResult = FormFieldTranslation.Create(languageCodeResult.Value, input.Label, input.Placeholder, input.HelpText);
            if (translationResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<FormFieldTranslation>>(translationResult.Error);
            }

            translations.Add(translationResult.Value);
        }

        return Result.Success<IReadOnlyList<FormFieldTranslation>>(translations);
    }
}
