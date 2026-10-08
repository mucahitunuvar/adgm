using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Search;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSearchSources;

// ADR-024 §10 (Faz 5 Görev 4). The response always lists "website" plus every currently registered
// IExternalSearchSource - including one that has never run yet (no SearchSourceState row), shown with
// null timestamps/error and a zero document count, rather than being silently left off the list.
public sealed class GetSearchSourcesQueryHandler(
    ISearchSourceStateRepository searchSourceStateRepository, IEnumerable<IExternalSearchSource> externalSearchSources)
    : IRequestHandler<GetSearchSourcesQuery, Result<IReadOnlyList<SearchSourceSummaryResponse>>>
{
    public async Task<Result<IReadOnlyList<SearchSourceSummaryResponse>>> Handle(
        GetSearchSourcesQuery request, CancellationToken cancellationToken)
    {
        var states = await searchSourceStateRepository.GetAllAsync(cancellationToken);
        var statesByKey = states.ToDictionary(s => s.SourceKey);

        var knownSourceKeys = new[] { WebsiteSearchIndexReconciler.WebsiteSourceKey }
            .Concat(externalSearchSources.Select(s => s.SourceKey))
            .Distinct()
            .OrderBy(key => key, StringComparer.Ordinal);

        var items = knownSourceKeys
            .Select(key => statesByKey.TryGetValue(key, out var state)
                ? new SearchSourceSummaryResponse(key, state.LastStartedAtUtc, state.LastSucceededAtUtc, state.LastError, state.DocumentCount)
                : new SearchSourceSummaryResponse(key, null, null, null, 0))
            .ToList();

        return Result.Success<IReadOnlyList<SearchSourceSummaryResponse>>(items);
    }
}
