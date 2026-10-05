using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface ILegalDocumentRepository
{
    Task<LegalDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LegalDocument?> GetByKeyAsync(LegalDocumentKey key, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LegalDocument>> GetAllAsync(CancellationToken cancellationToken = default);

    void Add(LegalDocument legalDocument);

    void Remove(LegalDocument legalDocument);
}
