using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class FormDefinitionRepository(WebsiteDbContext dbContext) : IFormDefinitionRepository
{
    public Task<FormDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.FormDefinitions.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<FormDefinition?> GetByKeyAsync(FormDefinitionKey key, CancellationToken cancellationToken = default) =>
        dbContext.FormDefinitions.FirstOrDefaultAsync(f => f.Key == key, cancellationToken);

    public async Task<IReadOnlyList<FormDefinition>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.FormDefinitions.AsNoTracking().OrderBy(f => f.Key).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<FormDefinition>> GetByLegalDocumentKeyAsync(
        LegalDocumentKey legalDocumentKey, CancellationToken cancellationToken = default) =>
        await dbContext.FormDefinitions
            .Where(f => f.PrivacyNoticeKey == legalDocumentKey || f.ExplicitConsents.Any(c => c.LegalDocumentKey == legalDocumentKey))
            .ToListAsync(cancellationToken);

    public void Add(FormDefinition formDefinition) => dbContext.FormDefinitions.Add(formDefinition);

    public void Remove(FormDefinition formDefinition) => dbContext.FormDefinitions.Remove(formDefinition);
}
