using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.LegalDocuments;

// ADR-024 §12.1/§12.2 (Faz 3 Görev 3): replaces the always-empty Görev 2 stub now that FormDefinition
// exists - a LegalDocument is in use when some FormDefinition references it as either its
// PrivacyNoticeKey or one of its ExplicitConsents. A single direct implementation (not a composite+
// providers fan-out) is enough since FormDefinition is currently the only source, mirroring
// VideoUsageChecker/SliderUsageChecker rather than CompositeMediaUsageChecker.
public sealed class LegalDocumentUsageChecker(
    ILegalDocumentRepository legalDocumentRepository, IFormDefinitionRepository formDefinitionRepository, ISiteLanguageRepository siteLanguageRepository)
    : ILegalDocumentUsageChecker
{
    public async Task<IReadOnlyList<LegalDocumentUsage>> GetUsagesAsync(Guid legalDocumentId, CancellationToken cancellationToken = default)
    {
        var legalDocument = await legalDocumentRepository.GetByIdAsync(legalDocumentId, cancellationToken);
        if (legalDocument is null)
        {
            return [];
        }

        var forms = await formDefinitionRepository.GetByLegalDocumentKeyAsync(legalDocument.Key, cancellationToken);
        if (forms.Count == 0)
        {
            return [];
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);

        return forms
            .Select(form =>
            {
                var translation = defaultLanguage is not null
                    ? form.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguage.Code)
                    : null;
                var title = (translation ?? form.Translations.FirstOrDefault())?.Title ?? form.Key.Value;
                var url = $"/admin/website/forms/{form.Id}";
                return new LegalDocumentUsage("form-definition", form.Id, $"Form: {title}", url);
            })
            .ToList();
    }
}
