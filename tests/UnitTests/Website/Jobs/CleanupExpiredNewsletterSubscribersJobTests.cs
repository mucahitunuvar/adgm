using GenclikMerkezi.Modules.Website;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.UnitTests.Website.Jobs;

// Hangfire runtime devreye girmez - job kendi IServiceScopeFactory'sini bir ServiceCollection'dan
// çözer, bu yüzden sahte repository buraya kaydedilir ve ExecuteAsync doğrudan çağrılır, aynı desen
// ArchiveClosedFormSubmissionsJobTests'te kullanılıyor.
public class CleanupExpiredNewsletterSubscribersJobTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LegalDocumentKey PrivacyKey = LegalDocumentKey.Create("newsletter-privacy").Value;
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeNewsletterSubscriberRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CleanupExpiredNewsletterSubscribersJob CreateJob(DateTime now)
    {
        var services = new ServiceCollection();
        services.AddSingleton<INewsletterSubscriberRepository>(_repository);
        services.AddKeyedSingleton<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey, _unitOfWork);
        var serviceProvider = services.BuildServiceProvider();

        return new CleanupExpiredNewsletterSubscribersJob(serviceProvider.GetRequiredService<IServiceScopeFactory>(), new FixedTimeProvider(now));
    }

    private static NewsletterSubscriber CreatePending(DateTime subscribedAtUtc) =>
        NewsletterSubscriber.Create(
            $"pending-{Guid.NewGuid():N}@example.com", Tr, PrivacyKey, 1, Guid.NewGuid().ToString("N"), subscribedAtUtc).Value;

    private static NewsletterSubscriber CreateUnsubscribed(DateTime unsubscribedAtUtc)
    {
        var subscriber = NewsletterSubscriber.Create(
            $"unsubscribed-{Guid.NewGuid():N}@example.com", Tr, PrivacyKey, 1, Guid.NewGuid().ToString("N"), unsubscribedAtUtc.AddDays(-1)).Value;
        subscriber.Confirm(unsubscribedAtUtc.AddDays(-1));
        subscriber.Unsubscribe(unsubscribedAtUtc);
        return subscriber;
    }

    [Fact]
    public async Task ExecuteAsync_RemovesPendingConfirmationSubscribedMoreThan7DaysAgo()
    {
        var subscriber = CreatePending(Now.AddDays(-8));
        _repository.Seed(subscriber);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.Null(await _repository.GetByIdAsync(subscriber.Id));
    }

    [Fact]
    public async Task ExecuteAsync_LeavesPendingConfirmationSubscribed6DaysAgoUntouched()
    {
        var subscriber = CreatePending(Now.AddDays(-6));
        _repository.Seed(subscriber);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.NotNull(await _repository.GetByIdAsync(subscriber.Id));
    }

    [Fact]
    public async Task ExecuteAsync_RemovesUnsubscribedMoreThan30DaysAgo()
    {
        var subscriber = CreateUnsubscribed(Now.AddDays(-31));
        _repository.Seed(subscriber);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.Null(await _repository.GetByIdAsync(subscriber.Id));
    }

    [Fact]
    public async Task ExecuteAsync_LeavesUnsubscribed29DaysAgoUntouched()
    {
        var subscriber = CreateUnsubscribed(Now.AddDays(-29));
        _repository.Seed(subscriber);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.NotNull(await _repository.GetByIdAsync(subscriber.Id));
    }

    [Fact]
    public async Task ExecuteAsync_NeverRemovesAnActiveSubscriber()
    {
        var subscriber = CreatePending(Now.AddDays(-100));
        subscriber.Confirm(Now.AddDays(-99));
        _repository.Seed(subscriber);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.NotNull(await _repository.GetByIdAsync(subscriber.Id));
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
