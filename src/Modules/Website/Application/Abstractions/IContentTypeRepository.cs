using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IContentTypeRepository
{
    Task<ContentType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ContentType?> GetByKeyAsync(ContentTypeKey key, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentType>> GetAllAsync(CancellationToken cancellationToken = default);

    // Cross-aggregate uniqueness check (ADR-024 §4.1): a language's RoutePrefix must be unique across
    // every ContentType, empty prefixes excluded - excludeId lets an update check against every OTHER
    // type without a query for "does this type collide with itself" always trivially succeeding.
    Task<bool> RoutePrefixExistsAsync(LanguageCode languageCode, string routePrefix, Guid? excludeId, CancellationToken cancellationToken = default);

    void Add(ContentType contentType);
}
