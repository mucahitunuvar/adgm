using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class LegalDocumentRepository(WebsiteDbContext dbContext) : ILegalDocumentRepository
{
    public Task<LegalDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.LegalDocuments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<LegalDocument?> GetByKeyAsync(LegalDocumentKey key, CancellationToken cancellationToken = default) =>
        dbContext.LegalDocuments.FirstOrDefaultAsync(d => d.Key == key, cancellationToken);

    public async Task<IReadOnlyList<LegalDocument>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.LegalDocuments.AsNoTracking().OrderBy(d => d.Key).ToListAsync(cancellationToken);

    public void Add(LegalDocument legalDocument) => dbContext.LegalDocuments.Add(legalDocument);

    public void Remove(LegalDocument legalDocument) => dbContext.LegalDocuments.Remove(legalDocument);
}
