using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IRedirectRepository
{
    Task<Redirect?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Redirect?> GetByFromPathAsync(LanguageCode languageCode, string fromPath, CancellationToken cancellationToken = default);

    Task<PagedResult<Redirect>> SearchAsync(
        LanguageCode? languageCode, bool? isAutomatic, string? search, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    void Add(Redirect redirect);

    void Remove(Redirect redirect);
}
