using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.UpsertEventSchedule;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.UpsertEventSchedule;

public class UpsertEventScheduleCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeContentTypeRepository _contentTypeRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly FakeEventScheduleRepository _eventScheduleRepository = new();
    private readonly FakeHtmlContentSanitizer _htmlContentSanitizer = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private UpsertEventScheduleCommandHandler CreateHandler() =>
        new(
            _contentItemRepository, _contentTypeRepository, _siteLanguageRepository, _eventScheduleRepository, _htmlContentSanitizer,
            new FakeCurrentUserContext(UserId), TimeProvider.System, _cacheService, new FakeUnitOfWork());

    private ContentItem SeedContentItem()
    {
        var defaultLanguage = SiteLanguage.Create(Tr, "Türkçe", 1, UserId, Now);
        defaultLanguage.MarkAsDefault(UserId, Now);
        _siteLanguageRepository.Seed(defaultLanguage);

        var flags = ContentTypeFeatureFlags.None with { SupportsEvent = true };
        var seo = SeoMetadata.Create(null, null, null, null, null, null, null, false).Value;
        var contentType = ContentType.Create(
            ContentTypeKey.Create("etkinlikler").Value, "list", "detail", ContentTypeSortMode.Manual, 1, flags, Tr, "Etkinlikler",
            "etkinlikler", seo, UserId, Now).Value;
        _contentTypeRepository.Seed(contentType);

        var contentItem = ContentItem.Create(
            contentType.Id, null, contentTypeSupportsDetailImage: false, 1, false, null, null, Tr, "Etkinlik", "etkinlik",
            "etkinlikler", [], null, null, seo, UserId, Now).Value;
        _contentItemRepository.Seed(contentItem);
        return contentItem;
    }

    private static UpsertEventScheduleCommand CreateCommand(Guid contentItemId, string format) =>
        new(
            contentItemId, null, Now.AddDays(10), Now.AddDays(10).AddHours(2), format, null, null, false, null, null, null, null,
            false, false, [new UpsertEventScheduleTranslationInput("tr", "Salon", null, null, null, null, null)]);

    [Theory]
    [InlineData("999")]
    [InlineData("InPerson,Online")] // 0|1 = 1 = Online - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedFormat_Fails(string format)
    {
        var contentItem = SeedContentItem();

        var result = await CreateHandler().Handle(CreateCommand(contentItem.Id, format), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("EventSchedule.InvalidFormat", result.Error.Code);
    }
}
