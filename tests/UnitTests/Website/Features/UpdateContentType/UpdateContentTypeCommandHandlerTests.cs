using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.CreateContentType;
using GenclikMerkezi.Modules.Website.Features.UpdateContentType;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.UpdateContentType;

public class UpdateContentTypeCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeContentTypeRepository _contentTypeRepository = new();
    private readonly FakeEventScheduleRepository _eventScheduleRepository = new();
    private readonly FakeSearchIndexUpdater _searchIndexUpdater = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private UpdateContentTypeCommandHandler CreateHandler() =>
        new(
            _contentTypeRepository, _eventScheduleRepository, _searchIndexUpdater, new FakeCurrentUserContext(UserId),
            _cacheService, new FakeUnitOfWork());

    private ContentType SeedContentType()
    {
        var seo = SeoMetadata.Create(null, null, null, null, null, null, null, false).Value;
        var contentType = ContentType.Create(
            ContentTypeKey.Create("haberler").Value, "list", "detail", ContentTypeSortMode.Manual, 1, ContentTypeFeatureFlags.None,
            Tr, "Haberler", "haberler", seo, UserId, Now).Value;
        _contentTypeRepository.Seed(contentType);
        return contentType;
    }

    private static UpdateContentTypeCommand CreateCommand(Guid id, byte[] rowVersion, string sortMode = "Manual", string? schemaKind = null) =>
        new(
            Id: id,
            RowVersion: rowVersion,
            ListTemplate: null,
            DetailTemplate: null,
            SortMode: sortMode,
            SortOrder: 1,
            SupportsHierarchy: false,
            SupportsCategories: false,
            SupportsTags: false,
            SupportsDetailImage: false,
            SupportsGallery: false,
            SupportsVideos: false,
            SupportsAttachments: false,
            SupportsEvent: false,
            SupportsBlockLayout: false,
            SupportsForm: false,
            SupportsRelatedContent: false,
            HasDetailPage: false,
            HasListingPage: false,
            IsSearchable: false,
            RequiresReview: false,
            SchemaKind: schemaKind);

    [Theory]
    [InlineData("999")]
    [InlineData("Manual,PublishDateDesc")] // 0|1 = 1 = PublishDateDesc - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedSortMode_Fails(string sortMode)
    {
        var contentType = SeedContentType();

        var result = await CreateHandler().Handle(CreateCommand(contentType.Id, contentType.RowVersion, sortMode), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentType.InvalidSortMode", result.Error.Code);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("None,Article")] // 0|1 = 1 = Article - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedSchemaKind_Fails(string schemaKind)
    {
        var contentType = SeedContentType();

        var result = await CreateHandler().Handle(
            CreateCommand(contentType.Id, contentType.RowVersion, schemaKind: schemaKind), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentType.InvalidSchemaKind", result.Error.Code);
    }
}
