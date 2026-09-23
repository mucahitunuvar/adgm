using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Support.Infrastructure.Jobs;

// Hangfire recurring job (Program.cs'te RecurringJob.AddOrUpdate<CloseOverdueSupportTicketsJob> ile
// saatlik kaydedilir). Hangfire job'ları bir HTTP request scope'u içinde çalışmaz, bu yüzden yalnızca
// singleton-güvenli bağımlılıklar (IServiceScopeFactory/IConfiguration) constructor'da alınır ve
// scoped servisler (repository/UnitOfWork/contract'lar) her çalıştırmada kendi açtığı scope'tan
// çözülür - test edilebilirlik için de bu tasarım tercih edildi (Hangfire runtime'ı olmadan, sınıf
// doğrudan örneklenip test edilebilir - bkz. CloseOverdueSupportTicketsJobTests).
public sealed class CloseOverdueSupportTicketsJob(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
{
    private const int DefaultSlaHours = 24;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var supportTicketRepository = scope.ServiceProvider.GetRequiredService<ISupportTicketRepository>();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var notificationModuleContract = scope.ServiceProvider.GetRequiredService<INotificationModuleContract>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(SupportModuleMarker.UnitOfWorkKey);

        var slaHours = configuration.GetValue("Support:SlaHours", DefaultSlaHours);
        var now = DateTime.UtcNow;
        var threshold = now.AddHours(-slaHours);

        var overdueTickets = await supportTicketRepository.GetOverdueOpenTicketsAsync(threshold, cancellationToken);

        if (overdueTickets.Count == 0)
        {
            return;
        }

        var closedTickets = new List<SupportTicket>();

        foreach (var ticket in overdueTickets)
        {
            var closeResult = ticket.Close(closedByUserId: null, reason: "SLA süresi doldu, otomatik kapatıldı.", now);

            if (closeResult.IsSuccess)
            {
                closedTickets.Add(ticket);
            }
        }

        if (closedTickets.Count == 0)
        {
            return;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var ticket in closedTickets)
        {
            var openerProfile = await identityService.GetUserProfileAsync(ticket.OpenedByUserId, cancellationToken);

            if (openerProfile is not null)
            {
                await notificationModuleContract.SendAsync(
                    openerProfile.UserId,
                    openerProfile.Email,
                    "Destek Talebiniz Kapatıldı",
                    $"\"{ticket.Subject}\" konulu destek talebiniz SLA süresi dolduğu için otomatik olarak kapatıldı.",
                    cancellationToken);
            }
        }
    }
}
