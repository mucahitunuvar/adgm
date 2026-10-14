using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeLegalDocumentRepository : ILegalDocumentRepository
{
    private readonly List<LegalDocument> _legalDocuments = [];

    public void Seed(LegalDocument legalDocument) => _legalDocuments.Add(legalDocument);

    public Task<LegalDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_legalDocuments.FirstOrDefault(d => d.Id == id));

    public Task<LegalDocument?> GetByKeyAsync(LegalDocumentKey key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_legalDocuments.FirstOrDefault(d => d.Key == key));

    public Task<IReadOnlyList<LegalDocument>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<LegalDocument>>(_legalDocuments.ToList());

    public void Add(LegalDocument legalDocument) => _legalDocuments.Add(legalDocument);

    public void Remove(LegalDocument legalDocument) => _legalDocuments.Remove(legalDocument);
}
