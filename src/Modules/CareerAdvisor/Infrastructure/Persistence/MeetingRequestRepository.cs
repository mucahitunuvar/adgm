using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.Modules.CareerAdvisor.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.CareerAdvisor.Infrastructure.Persistence;

public sealed class MeetingRequestRepository(CareerAdvisorDbContext dbContext) : IMeetingRequestRepository
{
    public Task<MeetingRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.MeetingRequests.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public void Add(MeetingRequest meetingRequest)
    {
        dbContext.MeetingRequests.Add(meetingRequest);
    }
}
