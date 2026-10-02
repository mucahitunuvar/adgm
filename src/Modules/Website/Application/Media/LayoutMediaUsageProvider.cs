using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// §4.3 "Kullanım koruması": registered into CompositeMediaUsageChecker - a MediaAsset referenced as a
// block's image (feature-mosaic, image-text, gallery, ...) in any PageLayout's draft or published
// blocks cannot be deleted.
public sealed class LayoutMediaUsageProvider(PageLayoutReferenceScanner scanner) : IMediaUsageProvider
{
    public async Task<IReadOnlyList<MediaUsage>> GetUsagesAsync(Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        var layouts = await scanner.FindReferencingAsync(refs => refs.ImageMediaIds.Contains(mediaAssetId), cancellationToken);

        return layouts
            .Select(layout => new MediaUsage(
                "page-layout", layout.Id, PageLayoutReferenceScanner.DescribeLayout(layout), PageLayoutReferenceScanner.DescribeLayoutUrl(layout)))
            .ToList();
    }
}
