using GenclikMerkezi.Modules.CareerAdvisor.Domain;

namespace GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;

public interface IMeetingRequestRepository
{
    Task<MeetingRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(MeetingRequest meetingRequest);
}
