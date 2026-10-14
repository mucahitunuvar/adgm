using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeFormDefinitionRepository : IFormDefinitionRepository
{
    private readonly List<FormDefinition> _formDefinitions = [];

    public void Seed(FormDefinition formDefinition) => _formDefinitions.Add(formDefinition);

    public Task<FormDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_formDefinitions.FirstOrDefault(f => f.Id == id));

    public Task<FormDefinition?> GetByKeyAsync(FormDefinitionKey key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_formDefinitions.FirstOrDefault(f => f.Key == key));

    public Task<IReadOnlyList<FormDefinition>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<FormDefinition>>(_formDefinitions.ToList());

    public Task<IReadOnlyList<FormDefinition>> GetByLegalDocumentKeyAsync(
        LegalDocumentKey legalDocumentKey, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<FormDefinition>>(_formDefinitions
            .Where(f => f.PrivacyNoticeKey == legalDocumentKey || f.ExplicitConsents.Any(c => c.LegalDocumentKey == legalDocumentKey))
            .ToList());

    public void Add(FormDefinition formDefinition) => _formDefinitions.Add(formDefinition);

    public void Remove(FormDefinition formDefinition) => _formDefinitions.Remove(formDefinition);
}
