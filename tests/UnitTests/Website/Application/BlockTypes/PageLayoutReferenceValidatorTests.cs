using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.BlockTypes;

public class PageLayoutReferenceValidatorTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();

    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeVideoRepository _videoRepository = new();
    private readonly FakeSliderRepository _sliderRepository = new();
    private readonly FakeImpactMetricRepository _impactMetricRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeContentTypeRepository _contentTypeRepository = new();
    private readonly FakeContentCategoryRepository _contentCategoryRepository = new();

    private PageLayoutReferenceValidator CreateValidator() =>
        new(
            _mediaAssetRepository, _videoRepository, _sliderRepository, _impactMetricRepository, _contentItemRepository,
            _contentTypeRepository, _contentCategoryRepository);

    private ContentType SeedContentType(string key, bool supportsEvent = false)
    {
        var contentType = ContentType.Create(
            ContentTypeKey.Create(key).Value, "list", "detail", ContentTypeSortMode.Manual, 1,
            ContentTypeFeatureFlags.None with { SupportsEvent = supportsEvent }, Tr, "Ad", "", EmptySeo, UserId, Now).Value;
        _contentTypeRepository.Seed(contentType);
        return contentType;
    }

    [Fact]
    public async Task ValidateAsync_MissingMediaAsset_Fails()
    {
        var result = await CreateValidator().ValidateAsync(
            [BlockReferenceSet.Empty with { ImageMediaIds = [Guid.NewGuid()] }], CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("NotFound", result.Error.Code);
    }

    [Fact]
    public async Task ValidateAsync_MediaAssetNotAnImage_Fails()
    {
        var id = Guid.NewGuid();
        var file = FileAttachment.Create($"website-documents/{id}.pdf", "a.pdf", "application/pdf", 10, Now, "MediaAsset", id);
        var document = MediaAsset.Create(id, MediaAssetKind.Document, file, [], null, null, MediaFolder.Create("docs").Value, null, null, false, UserId, Now).Value;
        _mediaAssetRepository.Seed(document);

        var result = await CreateValidator().ValidateAsync([BlockReferenceSet.Empty with { ImageMediaIds = [id] }], CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.ImageNotImage", result.Error.Code);
    }

    [Fact]
    public async Task ValidateAsync_MissingContentItem_Fails()
    {
        var result = await CreateValidator().ValidateAsync(
            [BlockReferenceSet.Empty with { ContentItemIds = [Guid.NewGuid()] }], CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.ContentItemNotFound", result.Error.Code);
    }

    [Fact]
    public async Task ValidateAsync_UnknownContentTypeListingId_Fails()
    {
        var result = await CreateValidator().ValidateAsync(
            [BlockReferenceSet.Empty with { ContentTypeListingIds = [Guid.NewGuid()] }], CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.ContentTypeNotFound", result.Error.Code);
    }

    [Fact]
    public async Task ValidateAsync_EventContentTypeKeyWithoutSupportsEvent_Fails()
    {
        SeedContentType("haber", supportsEvent: false);

        var result = await CreateValidator().ValidateAsync(
            [BlockReferenceSet.Empty with { EventContentTypeKeys = ["haber"] }], CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.ContentTypeDoesNotSupportEvent", result.Error.Code);
    }

    [Fact]
    public async Task ValidateAsync_EventContentTypeKeyWithSupportsEvent_Succeeds()
    {
        SeedContentType("etkinlik", supportsEvent: true);

        var result = await CreateValidator().ValidateAsync(
            [BlockReferenceSet.Empty with { EventContentTypeKeys = ["etkinlik"] }], CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateAsync_CategoryDoesNotBelongToContentType_Fails()
    {
        var typeA = SeedContentType("haber");
        var typeB = SeedContentType("duyuru");
        var category = ContentCategory.Create(typeB.Id, null, 1, Tr, "Kategori", null, EmptySeo, UserId, Now).Value;
        _contentCategoryRepository.Seed(category);

        var result = await CreateValidator().ValidateAsync(
            [BlockReferenceSet.Empty with { ContentTypeReferences = [new ContentTypeCategoryReference(typeA.Key.Value, category.Id)] }],
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.CategoryDoesNotBelongToContentType", result.Error.Code);
    }

    [Fact]
    public async Task ValidateAsync_CategoryBelongsToContentType_Succeeds()
    {
        var type = SeedContentType("haber");
        var category = ContentCategory.Create(type.Id, null, 1, Tr, "Kategori", null, EmptySeo, UserId, Now).Value;
        _contentCategoryRepository.Seed(category);

        var result = await CreateValidator().ValidateAsync(
            [BlockReferenceSet.Empty with { ContentTypeReferences = [new ContentTypeCategoryReference(type.Key.Value, category.Id)] }],
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
