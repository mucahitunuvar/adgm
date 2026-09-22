using GenclikMerkezi.Modules.Interview.Application.Abstractions;
using GenclikMerkezi.Modules.Interview.Domain;

namespace GenclikMerkezi.UnitTests.Interview.TestDoubles;

public sealed class FakeInterviewRepository : IInterviewRepository
{
    private readonly List<GenclikMerkezi.Modules.Interview.Domain.Interview> _interviews = [];

    public IReadOnlyCollection<GenclikMerkezi.Modules.Interview.Domain.Interview> Interviews => _interviews.AsReadOnly();

    public Task<GenclikMerkezi.Modules.Interview.Domain.Interview?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_interviews.FirstOrDefault(i => i.Id == id));

    public Task<IReadOnlyList<GenclikMerkezi.Modules.Interview.Domain.Interview>> GetByCandidateCvIdAsync(
        Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<GenclikMerkezi.Modules.Interview.Domain.Interview> matches =
            _interviews.Where(i => i.CandidateCvId == candidateCvId).ToList();
        return Task.FromResult(matches);
    }

    public Task<IReadOnlyList<GenclikMerkezi.Modules.Interview.Domain.Interview>> GetByCompanyIdAsync(
        Guid companyId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<GenclikMerkezi.Modules.Interview.Domain.Interview> matches =
            _interviews.Where(i => i.CompanyId == companyId).ToList();
        return Task.FromResult(matches);
    }

    public Task<IReadOnlyList<GenclikMerkezi.Modules.Interview.Domain.Interview>> GetPendingByOrganizingAdvisorIdAsync(
        Guid organizingAdvisorId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<GenclikMerkezi.Modules.Interview.Domain.Interview> matches = _interviews
            .Where(i => i.OrganizingAdvisorId == organizingAdvisorId
                && (i.Status == InterviewStatus.TalepEdildi || i.Status == InterviewStatus.Planlandi))
            .ToList();
        return Task.FromResult(matches);
    }

    public void Add(GenclikMerkezi.Modules.Interview.Domain.Interview interview)
    {
        _interviews.Add(interview);
    }
}
