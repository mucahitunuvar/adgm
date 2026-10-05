using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Forms;

// ADR-024 §12.2 (Faz 3 Görev 3): ContentItem.FormDefinitionId usage - the only source today. A later
// Görev (4/5, once FormSubmission exists) extends this with a second source, the same way
// VideoUsageChecker fans out across ContentItem and PageLayout.
public sealed class FormDefinitionUsageChecker(IContentItemRepository contentItemRepository, ISiteLanguageRepository siteLanguageRepository)
    : IFormDefinitionUsageChecker
{
    public async Task<IReadOnlyList<FormDefinitionUsage>> GetUsagesAsync(Guid formDefinitionId, CancellationToken cancellationToken = default)
    {
        var items = await contentItemRepository.GetByFormDefinitionIdAsync(formDefinitionId, cancellationToken);
        if (items.Count == 0)
        {
            return [];
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);

        return items
            .Select(item =>
            {
                var title = ResolveDisplayTitle(item, defaultLanguage);
                var url = $"/admin/website/content/{item.Id}";
                return new FormDefinitionUsage("content-item", item.Id, $"İçerik - Form: {title}", url);
            })
            .ToList();
    }

    private static string ResolveDisplayTitle(ContentItem item, SiteLanguage? defaultLanguage)
    {
        var translation = defaultLanguage is not null
            ? item.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguage.Code)
            : null;

        return (translation ?? item.Translations.FirstOrDefault())?.Title ?? item.Id.ToString();
    }
}
