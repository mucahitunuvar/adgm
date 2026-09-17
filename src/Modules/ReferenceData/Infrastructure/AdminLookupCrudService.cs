using GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure;

public sealed class AdminLookupCrudService<TLookup>(ReferenceDataDbContext dbContext) : IAdminLookupCrudService<TLookup>
    where TLookup : LookupItem
{
    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        dbContext.Set<TLookup>().AnyAsync(l => l.Code == code, cancellationToken);

    public Task<TLookup?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Set<TLookup>().FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public void Add(TLookup entity) => dbContext.Set<TLookup>().Add(entity);
}
