using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeSearchIndexUpdater : ISearchIndexUpdater
{
    public List<Guid> ReindexedContentItemIds { get; } = [];

    public List<Guid> ReindexedWithDescendantsContentItemIds { get; } = [];

    public List<Guid> RemovedContentItemIds { get; } = [];

    public List<Guid> ReindexedContentTypeIds { get; } = [];

    public List<LanguageCode> RemovedLanguages { get; } = [];

    public Task ReindexAsync(ContentItem contentItem, CancellationToken cancellationToken = default)
    {
        ReindexedContentItemIds.Add(contentItem.Id);
        return Task.CompletedTask;
    }

    public Task ReindexWithDescendantsAsync(ContentItem contentItem, CancellationToken cancellationToken = default)
    {
        ReindexedWithDescendantsContentItemIds.Add(contentItem.Id);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid contentItemId, CancellationToken cancellationToken = default)
    {
        RemovedContentItemIds.Add(contentItemId);
        return Task.CompletedTask;
    }

    public Task ReindexContentTypeAsync(Guid contentTypeId, CancellationToken cancellationToken = default)
    {
        ReindexedContentTypeIds.Add(contentTypeId);
        return Task.CompletedTask;
    }

    public Task RemoveLanguageAsync(LanguageCode languageCode, CancellationToken cancellationToken = default)
    {
        RemovedLanguages.Add(languageCode);
        return Task.CompletedTask;
    }
}
