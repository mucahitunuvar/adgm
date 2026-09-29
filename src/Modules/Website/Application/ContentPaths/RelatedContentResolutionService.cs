using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.ContentPaths;

// ADR-024 §4.1 (Faz 1b Görev 5): the three-step related content fallback. Introduced now, alongside
// the RelatedContentItemIds relationship itself, even though Görev 7's public detail response is its
// first real caller - the same "port ships with the relationship, wired up by a later Görev" pattern
// IVideoUsageChecker used in Görev 2/4.
//
// 1. Manually linked, visible, translated items - in the manually curated order, uncapped (the
//    caller's own list is already capped at ContentItem.MaxRelatedContent).
// 2. If (1) yields nothing: same ContentType, sharing at least one category, visible, translated,
//    newest effective publish date first, capped at 4.
// 3. If (1) and (2) both yield nothing: same ContentType (no category filter), same ordering/cap.
//
// The item itself is always excluded (by the repository's excludeId parameter) and an item without a
// translation in the requested language never appears in any step.
public sealed class RelatedContentResolutionService(IContentItemRepository contentItemRepository)
{
    private const int MaxAutomaticResults = 4;

    public async Task<IReadOnlyList<RelatedContentCandidate>> ResolveAsync(
        Guid contentItemId,
        Guid contentTypeId,
        IReadOnlyList<Guid> manualRelatedContentItemIds,
        IReadOnlyList<Guid> categoryIds,
        LanguageCode languageCode,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        if (manualRelatedContentItemIds.Count > 0)
        {
            var manualCandidates = await contentItemRepository.GetVisibleRelatedCandidatesByIdsAsync(
                manualRelatedContentItemIds, languageCode, now, cancellationToken);

            if (manualCandidates.Count > 0)
            {
                var candidatesById = manualCandidates.ToDictionary(c => c.Id);
                return manualRelatedContentItemIds
                    .Where(candidatesById.ContainsKey)
                    .Select(id => candidatesById[id])
                    .ToList();
            }
        }

        if (categoryIds.Count > 0)
        {
            var byCategoryCandidates = await contentItemRepository.SearchRelatedCandidatesAsync(
                contentTypeId, contentItemId, categoryIds, languageCode, now, MaxAutomaticResults, cancellationToken);

            if (byCategoryCandidates.Count > 0)
            {
                return byCategoryCandidates;
            }
        }

        return await contentItemRepository.SearchRelatedCandidatesAsync(
            contentTypeId, contentItemId, null, languageCode, now, MaxAutomaticResults, cancellationToken);
    }
}
