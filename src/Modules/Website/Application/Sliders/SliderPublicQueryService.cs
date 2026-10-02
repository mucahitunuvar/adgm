using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.LinkTargets;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.Sliders;

// §2 "Public: Ayrı bir public endpoint yok ... Bu görevde public yanıt için kullanılacak bir sorgu
// servisi yazılır": the single Application-layer service a future hero-slider block (Görev 4/5) will
// call to get a slider's visible, resolved slides - the same "service now, consumer later" shape
// LinkTargetResolver itself used in Görev 1. All of a slider's link targets are resolved in one batched
// call (§1.1 "N+1 yok"); image variant URLs are still resolved per slide, matching every other public
// handler's own (unbatched) MediaAsset lookup convention (GetPublicVideosQueryHandler,
// GetPublicContentByIdQueryHandler).
public sealed class SliderPublicQueryService(
    ISliderRepository sliderRepository,
    IMediaAssetRepository mediaAssetRepository,
    IFileStorageService fileStorageService,
    LinkTargetResolver linkTargetResolver)
{
    public async Task<IReadOnlyList<PublicSliderSlideResponse>> GetVisibleSlidesAsync(
        Guid sliderId, LanguageCode languageCode, LanguageCode defaultLanguageCode, DateTime now, CancellationToken cancellationToken = default)
    {
        var slider = await sliderRepository.GetByIdAsync(sliderId, cancellationToken);
        if (slider is null)
        {
            return [];
        }

        // §1 "Çeviri kuralı": a slide with no translation in languageCode is not shown, no fallback to
        // the default language.
        var candidates = slider.Slides
            .Where(s => SlideVisibility.Evaluate(s, now))
            .OrderBy(s => s.SortOrder)
            .Select(s => (Slide: s, Translation: s.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)))
            .Where(c => c.Translation is not null)
            .ToList();

        var targets = candidates.Select(c => c.Slide.LinkTarget).Where(t => !t.IsEmpty).ToList();
        var resolutions = await linkTargetResolver.ResolveManyAsync(targets, languageCode, defaultLanguageCode, now, cancellationToken);

        var results = new List<PublicSliderSlideResponse>();
        foreach (var (slide, translation) in candidates)
        {
            var desktopAsset = await mediaAssetRepository.GetByIdAsync(slide.DesktopImageMediaId, cancellationToken);
            if (desktopAsset is null)
            {
                // MediaImageReferenceGuard/SliderMediaUsageProvider should make this unreachable - a
                // desktop image cannot be deleted while a slide still references it - but a missing
                // image leaves nothing sensible to render, so the slide is skipped rather than shown
                // broken.
                continue;
            }

            var desktopImage = await ToImageResponseAsync(desktopAsset, cancellationToken);

            PublicSliderImageResponse? mobileImage = null;
            if (slide.MobileImageMediaId is { } mobileImageId)
            {
                var mobileAsset = await mediaAssetRepository.GetByIdAsync(mobileImageId, cancellationToken);
                mobileImage = mobileAsset is null ? null : await ToImageResponseAsync(mobileAsset, cancellationToken);
            }

            var mediaTranslation = desktopAsset.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            var altText = GalleryItemDisplayResolver.ResolveAltText(translation!.AltTextOverride, mediaTranslation?.AltText);

            string? buttonLabel = null;
            string? buttonHref = null;
            if (!string.IsNullOrEmpty(translation.ButtonLabel)
                && resolutions.TryGetValue(slide.LinkTarget, out var resolution) && resolution.IsResolved)
            {
                buttonLabel = translation.ButtonLabel;
                buttonHref = resolution.Href;
            }

            results.Add(new PublicSliderSlideResponse(
                slide.Id, translation.Eyebrow, translation.Title, translation.Text, desktopImage, mobileImage, altText, buttonLabel,
                buttonHref));
        }

        return results;
    }

    private async Task<PublicSliderImageResponse> ToImageResponseAsync(MediaAsset mediaAsset, CancellationToken cancellationToken)
    {
        var originalUrl = await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);
        string? small = null;
        string? medium = null;
        string? large = null;

        foreach (var variant in mediaAsset.Variants)
        {
            var url = await fileStorageService.GetUrlAsync(variant.File.FileKey, cancellationToken);
            if (variant.VariantName == MediaAssetVariantNames.Small)
            {
                small = url;
            }
            else if (variant.VariantName == MediaAssetVariantNames.Medium)
            {
                medium = url;
            }
            else if (variant.VariantName == MediaAssetVariantNames.Large)
            {
                large = url;
            }
        }

        return new PublicSliderImageResponse(small, medium, large, originalUrl);
    }
}
