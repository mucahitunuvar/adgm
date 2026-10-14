using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Features.GetPublicEvents;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.GetPublicEvents;

public class GetPublicEventsQueryHandlerTests
{
    private readonly FakeContentTypeRepository _contentTypeRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly FakeEventScheduleRepository _eventScheduleRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeMediaFileStorageService _fileStorageService = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private GetPublicEventsQueryHandler CreateHandler() =>
        new(
            _contentTypeRepository, _siteLanguageRepository, _eventScheduleRepository, _contentItemRepository, _mediaAssetRepository,
            _fileStorageService, _cacheService, TimeProvider.System);

    [Theory]
    [InlineData("999")]
    [InlineData("InPerson,Online")] // 0|1 = 1 = Online - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedFormat_Fails(string format)
    {
        var result = await CreateHandler().Handle(
            new GetPublicEventsQuery(null, null, null, null, format, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Event.InvalidFormat", result.Error.Code);
    }
}
