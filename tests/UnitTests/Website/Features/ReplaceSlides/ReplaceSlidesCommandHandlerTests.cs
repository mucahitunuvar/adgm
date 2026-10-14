using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.ReplaceSlides;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.ReplaceSlides;

public class ReplaceSlidesCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeSliderRepository _sliderRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private ReplaceSlidesCommandHandler CreateHandler()
    {
        var defaultLanguage = SiteLanguage.Create(Tr, "Türkçe", 1, UserId, Now);
        defaultLanguage.MarkAsDefault(UserId, Now);
        _siteLanguageRepository.Seed(defaultLanguage);

        return new ReplaceSlidesCommandHandler(
            _sliderRepository, _siteLanguageRepository, _mediaAssetRepository, new FakeCurrentUserContext(UserId), TimeProvider.System,
            _cacheService, new FakeUnitOfWork());
    }

    private MediaAsset SeedImage()
    {
        var file = FileAttachment.Create(
            "website-images/2026/10/01/slide.jpg", "slide.jpg", "image/jpeg", 1024, Now, "MediaAsset", UserId);
        var mediaAsset = MediaAsset.Create(
            UserId, MediaAssetKind.Image, file, [], 1200, 600, MediaFolder.Create("slides").Value, null, null, false, UserId, Now).Value;
        _mediaAssetRepository.Seed(mediaAsset);
        return mediaAsset;
    }

    [Theory]
    [InlineData("999")]
    [InlineData("None,Content")] // 0|1 = 1 = Content - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedLinkKind_Fails(string linkKind)
    {
        var image = SeedImage();
        var slider = Slider.Create("ana-slider", Tr, "Ana Slider", UserId, Now).Value;
        _sliderRepository.Seed(slider);

        var slides = new[]
        {
            new SlideInput(
                image.Id, null, new SlideLinkInput(linkKind, null, null, null, null), 1, true, null, null,
                [new SlideTranslationInput("tr", null, "Başlık", null, null, null)]),
        };
        var command = new ReplaceSlidesCommand(slider.Id, slider.RowVersion, slides);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Slide.LinkKindInvalid", result.Error.Code);
    }
}
