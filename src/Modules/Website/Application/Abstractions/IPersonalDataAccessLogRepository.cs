using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IPersonalDataAccessLogRepository
{
    void Add(PersonalDataAccessLog accessLog);

    Task<PagedResult<PersonalDataAccessLog>> SearchAsync(
        DateTime? fromUtc, DateTime? toUtc, PagedRequest pagedRequest, CancellationToken cancellationToken = default);
}
