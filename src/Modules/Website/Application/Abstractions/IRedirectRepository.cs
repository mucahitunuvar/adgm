using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IRedirectRepository
{
    Task<Redirect?> GetByFromPathAsync(LanguageCode languageCode, string fromPath, CancellationToken cancellationToken = default);

    void Add(Redirect redirect);

    void Remove(Redirect redirect);
}
