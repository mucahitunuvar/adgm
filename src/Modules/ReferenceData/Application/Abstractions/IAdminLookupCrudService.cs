using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;

// Not the generic repository ADR-009 forbids: this is not usable for arbitrary entities, only for
// the one bounded concern ADR-016 already scoped (admin-managed lookup CRUD), constrained to
// LookupItem - the same kind of scoped, real domain need ADR-009 itself carves out for
// ICandidateRepository-style repositories, parameterized over TLookup instead of one fixed type.
// Constrained only to LookupItem (not ILookupItemFactory<TLookup>) so TaxOffice can reuse it too -
// TaxOffice's own Create needs a ProvinceId the shared factory interface has no slot for, so it
// builds its entity itself and only needs this service's CodeExistsAsync/FindByIdAsync/Add.
public interface IAdminLookupCrudService<TLookup>
    where TLookup : LookupItem
{
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

    Task<TLookup?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(TLookup entity);
}
