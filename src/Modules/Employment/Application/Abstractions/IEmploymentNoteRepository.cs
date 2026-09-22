using GenclikMerkezi.Modules.Employment.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Employment.Application.Abstractions;

public interface IEmploymentNoteRepository
{
    void Add(EmploymentNote employmentNote);

    Task<PagedResult<EmploymentNote>> GetByEmploymentIdAsync(
        Guid employmentId, PagedRequest pagedRequest, CancellationToken cancellationToken = default);
}
