using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Jobs;

// Hangfire runtime devreye girmez - job kendi IServiceScopeFactory'sini bir ServiceCollection'dan
// çözer, bu yüzden sahte repository'ler buraya kaydedilir ve ExecuteAsync doğrudan çağrılır (aynı
// desen CleanupStaleNotFoundLogsJobTests'te kullanılıyor).
public class PermanentlyDeleteExpiredTrashJobTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeRedirectRepository _redirectRepository = new();
    private readonly FakeMenuRepository _menuRepository = new();
    private readonly FakePageLayoutRepository _pageLayoutRepository = new();
    private readonly FakeEventScheduleRepository _eventScheduleRepository = new();
    private readonly FakeEventRegistrationUsageChecker _eventRegistrationUsageChecker = new();
    private readonly FakeContentItemRevisionRepository _contentItemRevisionRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private PermanentlyDeleteExpiredTrashJob CreateJob(DateTime now)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IContentItemRepository>(_contentItemRepository);
        services.AddSingleton<IRedirectRepository>(_redirectRepository);
        services.AddSingleton<IMenuRepository>(_menuRepository);
        services.AddSingleton<IPageLayoutRepository>(_pageLayoutRepository);
        services.AddSingleton<IEventScheduleRepository>(_eventScheduleRepository);
        services.AddSingleton<IEventRegistrationUsageChecker>(_eventRegistrationUsageChecker);
        services.AddSingleton<IContentItemRevisionRepository>(_contentItemRevisionRepository);
        services.AddSingleton<ContentItemPermanentDeletionService>();
        services.AddSingleton<ILogger<ContentItemPermanentDeletionService>>(NullLogger<ContentItemPermanentDeletionService>.Instance);
        services.AddSingleton<ICacheService>(
            new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings())));
        services.AddKeyedSingleton<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey, _unitOfWork);
        var serviceProvider = services.BuildServiceProvider();

        return new PermanentlyDeleteExpiredTrashJob(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(), new FixedTimeProvider(now));
    }

    private static ContentItem CreateTrashedItem(Guid? parentId, string slug, DateTime deletedAtUtc)
    {
        var item = ContentItem.Create(
            ContentTypeId, parentId, false, 1, false, null, null, Tr, "Başlık", slug, "haberler", [], null, "<p/>", EmptySeo, UserId,
            deletedAtUtc).Value;
        item.MoveToTrash(UserId, deletedAtUtc);
        return item;
    }

    [Fact]
    public async Task ExecuteAsync_DeletesLeafItemOlderThanRetentionWindow()
    {
        var expired = CreateTrashedItem(null, "eski", Now.AddDays(-(ContentItem.TrashRetentionDays + 1)));
        _contentItemRepository.Seed(expired);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.Null(await _contentItemRepository.GetByIdAsync(expired.Id));
    }

    [Fact]
    public async Task ExecuteAsync_LeavesRecentlyTrashedItemsUntouched()
    {
        var recent = CreateTrashedItem(null, "yeni", Now.AddDays(-1));
        _contentItemRepository.Seed(recent);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.NotNull(await _contentItemRepository.GetByIdAsync(recent.Id));
    }

    [Fact]
    public async Task ExecuteAsync_SkipsExpiredItemThatStillHasAChild()
    {
        var parent = CreateTrashedItem(null, "ust", Now.AddDays(-(ContentItem.TrashRetentionDays + 1)));
        _contentItemRepository.Seed(parent);
        var child = CreateTrashedItem(parent.Id, "alt", Now.AddDays(-(ContentItem.TrashRetentionDays + 1)));
        _contentItemRepository.Seed(child);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        // Candidates are processed in seed order (parent, then child): when the parent is evaluated
        // it still has its child, so it is skipped this run - the leaf child is deleted instead, and
        // the parent becomes eligible on a subsequent run once it truly has no children left.
        Assert.NotNull(await _contentItemRepository.GetByIdAsync(parent.Id));
        Assert.Null(await _contentItemRepository.GetByIdAsync(child.Id));
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
