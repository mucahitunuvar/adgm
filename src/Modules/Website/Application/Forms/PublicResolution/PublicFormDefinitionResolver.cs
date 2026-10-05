using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Forms.PublicResolution;

// ADR-024 §12.2: builds the public shape of an active FormDefinition - shared between the content
// detail endpoint's embedded `form` field (Görev 3, ADR-024 §12.2 "aynı biçimde") and Görev 4's own
// GET /api/v1/public/forms/{key}, the same way PublicPageLayoutResolver is shared across several public
// read endpoints. Returns null whenever nothing complete can be shown - no translation for languageCode,
// or the privacy notice/any explicit consent document has no version currently in force - never a
// partially-filled response.
public sealed class PublicFormDefinitionResolver(ILegalDocumentRepository legalDocumentRepository)
{
    public async Task<PublicFormDefinitionResponse?> ResolveAsync(
        FormDefinition form, LanguageCode languageCode, DateTime now, CancellationToken cancellationToken)
    {
        var translation = form.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (translation is null)
        {
            return null;
        }

        var privacyNotice = await ResolveLegalDocumentReferenceAsync(form.PrivacyNoticeKey, isRequired: true, languageCode, now, cancellationToken);
        if (privacyNotice is null)
        {
            return null;
        }

        // §12.2 "Form ancak bağlı aydınlatma metninin (ve zorunlu açık rızaların) yürürlükte bir sürümü
        // varsa aktif edilebilir": only a REQUIRED consent's missing effective version hides the whole
        // form (it could never be legally submitted) - an optional consent without one is simply
        // omitted from the list, the same leniency FormDefinition.Activate itself applies.
        var explicitConsents = new List<PublicFormLegalDocumentReferenceResponse>();
        foreach (var consent in form.ExplicitConsents)
        {
            var reference = await ResolveLegalDocumentReferenceAsync(
                consent.LegalDocumentKey, consent.IsRequired, languageCode, now, cancellationToken);
            if (reference is null)
            {
                if (consent.IsRequired)
                {
                    return null;
                }

                continue;
            }

            explicitConsents.Add(reference);
        }

        var fields = form.Fields
            .OrderBy(f => f.SortOrder)
            .Select(f => BuildFieldResponse(f, languageCode))
            .ToList();

        return new PublicFormDefinitionResponse(
            form.Key.Value, translation.Title, translation.Description, translation.SubmitButtonLabel, fields, privacyNotice, explicitConsents);
    }

    private async Task<PublicFormLegalDocumentReferenceResponse?> ResolveLegalDocumentReferenceAsync(
        LegalDocumentKey key, bool isRequired, LanguageCode languageCode, DateTime now, CancellationToken cancellationToken)
    {
        var document = await legalDocumentRepository.GetByKeyAsync(key, cancellationToken);
        if (document is null)
        {
            return null;
        }

        var effectiveVersion = LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, now);
        if (effectiveVersion is null)
        {
            return null;
        }

        var titleTranslation = document.Translations.FirstOrDefault(t => t.LanguageCode == languageCode) ?? document.Translations.FirstOrDefault();
        var title = titleTranslation?.Title ?? document.Key.Value;

        return new PublicFormLegalDocumentReferenceResponse(document.Key.Value, effectiveVersion.VersionNumber, title, isRequired);
    }

    private static PublicFormFieldResponse BuildFieldResponse(FormField field, LanguageCode languageCode)
    {
        var translation = field.Translations.FirstOrDefault(t => t.LanguageCode == languageCode) ?? field.Translations.FirstOrDefault();
        var label = translation?.Label ?? field.Key;

        var options = field.Options
            .Select(o =>
            {
                var optionTranslation = o.Translations.FirstOrDefault(t => t.LanguageCode == languageCode) ?? o.Translations.FirstOrDefault();
                return new PublicFormFieldOptionResponse(o.Key, optionTranslation?.Label ?? o.Key);
            })
            .ToList();

        var allowedFileTypes = field.AllowedFileTypes.Select(t => t.ToString()).ToList();

        return new PublicFormFieldResponse(
            field.Key, field.Type.ToString(), label, translation?.Placeholder, translation?.HelpText, field.IsRequired, field.MinLength,
            field.MaxLength, options, field.DateMin, field.DateMax, allowedFileTypes, field.MaxSizeMb);
    }
}
