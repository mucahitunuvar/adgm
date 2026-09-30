using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeContentTypeRepository : IContentTypeRepository
{
    private readonly List<ContentType> _contentTypes = [];

    public void Seed(ContentType contentType) => _contentTypes.Add(contentType);

    public Task<ContentType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_contentTypes.FirstOrDefault(t => t.Id == id));

    public Task<ContentType?> GetByKeyAsync(ContentTypeKey key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_contentTypes.FirstOrDefault(t => t.Key == key));

    public Task<IReadOnlyList<ContentType>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentType>>(_contentTypes.ToList());

    public Task<ContentType?> GetByRoutePrefixAsync(LanguageCode languageCode, string routePrefix, CancellationToken cancellationToken = default) =>
        Task.FromResult(_contentTypes.FirstOrDefault(
            t => t.Translations.Any(tr => tr.LanguageCode == languageCode && tr.RoutePrefix == routePrefix)));

    public Task<bool> RoutePrefixExistsAsync(
        LanguageCode languageCode, string routePrefix, Guid? excludeId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_contentTypes
            .Where(t => excludeId == null || t.Id != excludeId.Value)
            .Any(t => t.Translations.Any(tr => tr.LanguageCode == languageCode && tr.RoutePrefix == routePrefix)));

    public void Add(ContentType contentType) => _contentTypes.Add(contentType);
}
