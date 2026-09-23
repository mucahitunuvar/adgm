using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.Support;
using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.Modules.Support.Infrastructure.Jobs;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.Support.TestDoubles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.UnitTests.Support.Jobs;

// Hangfire runtime'ı olmadan (RecurringJob.AddOrUpdate/BackgroundJobServer hiç devreye girmez) -
// sınıf doğrudan örneklenip ExecuteAsync çağrılır. Job kendi IServiceScopeFactory'sini bir
// ServiceCollection'dan oluşturduğu için, çözülecek servisler (repository/UnitOfWork/contract'lar)
// buraya sahte implementasyonlar olarak kaydedilir - tıpkı gerçek DI container'ının Support modülü
// için yapacağı gibi (bkz. SupportModuleServiceCollectionExtensions).
public class CloseOverdueSupportTicketsJobTests
{
    private readonly FakeSupportTicketRepository _supportTicketRepository = new();
    private readonly FakeIdentityService _identityService = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CloseOverdueSupportTicketsJob CreateJob(int slaHours = 24)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISupportTicketRepository>(_supportTicketRepository);
        services.AddSingleton<IIdentityService>(_identityService);
        services.AddSingleton<GenclikMerkezi.Contracts.Notification.INotificationModuleContract>(_notificationModuleContract);
        services.AddKeyedSingleton<IUnitOfWork>(SupportModuleMarker.UnitOfWorkKey, _unitOfWork);
        var serviceProvider = services.BuildServiceProvider();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([new KeyValuePair<string, string?>("Support:SlaHours", slaHours.ToString())])
            .Build();

        return new CloseOverdueSupportTicketsJob(serviceProvider.GetRequiredService<IServiceScopeFactory>(), configuration);
    }

    private SupportTicket SeedOpenTicket(Guid openedByUserId, DateTime openedSinceUtc)
    {
        var ticket = SupportTicket.Create(
            SupportTicketOpenerRole.Candidate, openedByUserId, Guid.NewGuid(), null,
            "Konu", SupportTicketPriority.Orta, Guid.NewGuid(), openedSinceUtc);
        _supportTicketRepository.Add(ticket);
        return ticket;
    }

    [Fact]
    public async Task ExecuteAsync_ClosesOverdueOpenTickets_AndNotifiesOpener()
    {
        var openerUserId = Guid.NewGuid();
        var ticket = SeedOpenTicket(openerUserId, DateTime.UtcNow.AddHours(-25));
        _identityService.UserProfilesById[openerUserId] =
            new IdentityUserProfile(openerUserId, "aday@example.com", "Ahmet", "Yılmaz", null);

        await CreateJob(slaHours: 24).ExecuteAsync(CancellationToken.None);

        Assert.Equal(SupportTicketStatus.Kapandi, ticket.Status);
        Assert.Null(ticket.ClosedByUserId);
        Assert.Equal("SLA süresi doldu, otomatik kapatıldı.", ticket.ClosedReason);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        var notification = Assert.Single(_notificationModuleContract.SentNotifications);
        Assert.Equal(openerUserId, notification.UserId);
    }

    [Fact]
    public async Task ExecuteAsync_LeavesTicketsWithinSlaThreshold_Untouched()
    {
        var ticket = SeedOpenTicket(Guid.NewGuid(), DateTime.UtcNow.AddHours(-1));

        await CreateJob(slaHours: 24).ExecuteAsync(CancellationToken.None);

        Assert.Equal(SupportTicketStatus.Acik, ticket.Status);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.Empty(_notificationModuleContract.SentNotifications);
    }

    [Fact]
    public async Task ExecuteAsync_LeavesAlreadyAnsweredTickets_Untouched()
    {
        var ticket = SeedOpenTicket(Guid.NewGuid(), DateTime.UtcNow.AddHours(-48));
        ticket.MarkAnswered(DateTime.UtcNow.AddHours(-48));

        await CreateJob(slaHours: 24).ExecuteAsync(CancellationToken.None);

        Assert.Equal(SupportTicketStatus.Cevaplandi, ticket.Status);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoOverdueTickets_DoesNotCallSaveChanges()
    {
        await CreateJob().ExecuteAsync(CancellationToken.None);

        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
