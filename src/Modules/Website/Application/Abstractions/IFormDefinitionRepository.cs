using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IFormDefinitionRepository
{
    Task<FormDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<FormDefinition?> GetByKeyAsync(FormDefinitionKey key, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FormDefinition>> GetAllAsync(CancellationToken cancellationToken = default);

    // LegalDocumentUsageChecker's real implementation (Faz 3 Görev 3): every FormDefinition whose
    // PrivacyNoticeKey or any ExplicitConsents entry references legalDocumentKey.
    Task<IReadOnlyList<FormDefinition>> GetByLegalDocumentKeyAsync(LegalDocumentKey legalDocumentKey, CancellationToken cancellationToken = default);

    void Add(FormDefinition formDefinition);

    void Remove(FormDefinition formDefinition);
}
