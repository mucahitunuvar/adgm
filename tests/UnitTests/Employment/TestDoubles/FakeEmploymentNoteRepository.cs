using GenclikMerkezi.Modules.Employment.Application.Abstractions;
using GenclikMerkezi.Modules.Employment.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Employment.TestDoubles;

public sealed class FakeEmploymentNoteRepository : IEmploymentNoteRepository
{
    private readonly List<EmploymentNote> _employmentNotes = [];

    public IReadOnlyCollection<EmploymentNote> EmploymentNotes => _employmentNotes.AsReadOnly();

    public void Add(EmploymentNote employmentNote)
    {
        _employmentNotes.Add(employmentNote);
    }

    public Task<PagedResult<EmploymentNote>> GetByEmploymentIdAsync(
        Guid employmentId, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var matches = _employmentNotes
            .Where(n => n.EmploymentId == employmentId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToList();

        var items = matches
            .Skip((pagedRequest.Page - 1) * pagedRequest.PageSize)
            .Take(pagedRequest.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<EmploymentNote>(items, matches.Count, pagedRequest.Page, pagedRequest.PageSize));
    }
}
