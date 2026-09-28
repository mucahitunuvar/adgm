using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class RedirectRepository(WebsiteDbContext dbContext) : IRedirectRepository
{
    public Task<Redirect?> GetByFromPathAsync(LanguageCode languageCode, string fromPath, CancellationToken cancellationToken = default) =>
        dbContext.Redirects.FirstOrDefaultAsync(r => r.LanguageCode == languageCode && r.FromPath == fromPath, cancellationToken);

    public void Add(Redirect redirect) => dbContext.Redirects.Add(redirect);

    public void Remove(Redirect redirect) => dbContext.Redirects.Remove(redirect);
}
