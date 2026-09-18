using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.Modules.CareerAdvisor.Domain;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

public sealed class FakeMeetingRequestRepository : IMeetingRequestRepository
{
    private readonly List<MeetingRequest> _meetingRequests = [];

    public IReadOnlyCollection<MeetingRequest> MeetingRequests => _meetingRequests.AsReadOnly();

    public Task<MeetingRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_meetingRequests.FirstOrDefault(m => m.Id == id));

    public void Add(MeetingRequest meetingRequest)
    {
        _meetingRequests.Add(meetingRequest);
    }
}
