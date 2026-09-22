using GenclikMerkezi.Modules.Employment.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.Employment.TestDoubles;

public sealed class FakeEmploymentRepository : IEmploymentRepository
{
    private readonly List<GenclikMerkezi.Modules.Employment.Domain.Employment> _employments = [];

    public IReadOnlyCollection<GenclikMerkezi.Modules.Employment.Domain.Employment> Employments => _employments.AsReadOnly();

    public Task<GenclikMerkezi.Modules.Employment.Domain.Employment?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_employments.FirstOrDefault(e => e.Id == id));

    public Task<IReadOnlyList<GenclikMerkezi.Modules.Employment.Domain.Employment>> GetByCandidateCvIdAsync(
        Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<GenclikMerkezi.Modules.Employment.Domain.Employment> matches =
            _employments.Where(e => e.CandidateCvId == candidateCvId).ToList();
        return Task.FromResult(matches);
    }

    public void Add(GenclikMerkezi.Modules.Employment.Domain.Employment employment)
    {
        _employments.Add(employment);
    }
}
