using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.CareerAdvisor.Features.ConfirmMeetingRequest;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateMeetingRequest;
using GenclikMerkezi.Modules.CareerAdvisor.Features.SendBulkNotification;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.CareerAdvisor.Infrastructure;

// GetActiveAdvisorsAsync'in aksine (ki o saf bir okuma projeksiyonu olduğu için doğrudan
// DbContext'e sorgu atıyor), CreateMeetingRequestAsync/ConfirmMeetingRequestAsync gerçek iş
// mantığı + bildirim içerdiği için Identity'nin IdentityService'i gibi kendi modülünün iç
// MediatR komutlarına delege ediyor (Görev 5/ADR-022 §4).
public sealed class CareerAdvisorModuleContract(CareerAdvisorDbContext dbContext, ISender sender) : ICareerAdvisorModuleContract
{
    public async Task<IReadOnlyList<ActiveCareerAdvisorSummary>> GetActiveAdvisorsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CareerAdvisors
            .AsNoTracking()
            .Where(a => a.IsActive)
            .Select(a => new ActiveCareerAdvisorSummary(a.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<Result<Guid>> CreateMeetingRequestAsync(
        Guid candidateCvId, Guid candidateUserId, Guid careerAdvisorId, CancellationToken cancellationToken = default)
    {
        return sender.Send(new CreateMeetingRequestCommand(candidateCvId, candidateUserId, careerAdvisorId), cancellationToken);
    }

    public Task<Result> ConfirmMeetingRequestAsync(
        Guid meetingRequestId, Guid candidateUserId, CancellationToken cancellationToken = default)
    {
        return sender.Send(new ConfirmMeetingRequestCommand(meetingRequestId, candidateUserId), cancellationToken);
    }

    public async Task<Guid?> GetAdvisorIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.CareerAdvisors
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.IsActive)
            .Select(a => (Guid?)a.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task SendBulkNotificationAsync(
        IReadOnlyList<Guid> candidateUserIds, string subject, string message, CancellationToken cancellationToken = default)
    {
        return sender.Send(new SendBulkNotificationCommand(candidateUserIds, subject, message), cancellationToken);
    }
}
