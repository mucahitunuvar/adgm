using System.Text.Json;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// §4.3 "Kullanım koruması: Bloklarin (taslak ve yayındaki) referans verdiği medya, video, slider ve
// göstergeler silinemez" - the shared sweep behind SliderUsageChecker, VideoUsageChecker's layout-aware
// extension, and LayoutMediaUsageProvider. Every PageLayout is small (at most PageLayout.MaxBlocks
// blocks per list) and there are few of them (one Home plus one per SupportsBlockLayout content item),
// so this is a full in-memory scan, not a targeted query - the same volume assumption
// IPageLayoutRepository.GetAllAsync documents.
public sealed class PageLayoutReferenceScanner(IPageLayoutRepository pageLayoutRepository, IBlockTypeRegistry blockTypeRegistry)
{
    public async Task<IReadOnlyList<PageLayout>> FindReferencingAsync(
        Func<BlockReferenceSet, bool> matches, CancellationToken cancellationToken)
    {
        var layouts = await pageLayoutRepository.GetAllAsync(cancellationToken);

        return layouts
            .Where(layout => References(layout.DraftBlocks, matches) || References(layout.PublishedBlocks, matches))
            .ToList();
    }

    public static string DescribeLayout(PageLayout layout) =>
        layout.TargetKind == PageLayoutTargetKind.Home
            ? "Sayfa Düzeni - Ana Sayfa"
            : $"Sayfa Düzeni - İçerik {layout.ContentItemId}";

    public static string DescribeLayoutUrl(PageLayout layout) =>
        layout.TargetKind == PageLayoutTargetKind.Home
            ? "/admin/website/layouts/home"
            : $"/admin/website/layouts/content/{layout.ContentItemId}";

    private bool References(IReadOnlyList<LayoutBlock> blocks, Func<BlockReferenceSet, bool> matches) =>
        blocks.Any(block => matches(ExtractReferences(block)));

    private BlockReferenceSet ExtractReferences(LayoutBlock block)
    {
        var definition = blockTypeRegistry.TryGet(block.BlockTypeKey);
        if (definition is null)
        {
            return BlockReferenceSet.Empty;
        }

        var settingsElement = JsonSerializer.Deserialize<JsonElement>(block.SettingsJson);
        var textsByLanguage = block.Translations.ToDictionary(t => t.LanguageCode, t => JsonSerializer.Deserialize<JsonElement>(t.TextsJson));

        return definition.ExtractReferences(settingsElement, textsByLanguage);
    }
}
