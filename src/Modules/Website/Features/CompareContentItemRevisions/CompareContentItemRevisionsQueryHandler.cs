using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentRevisions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CompareContentItemRevisions;

public sealed class CompareContentItemRevisionsQueryHandler(
    IContentItemRepository contentItemRepository,
    IContentItemRevisionRepository contentItemRevisionRepository,
    ISiteLanguageRepository siteLanguageRepository)
    : IRequestHandler<CompareContentItemRevisionsQuery, Result<CompareContentItemRevisionsResponse>>
{
    private static readonly Error TranslationNotFoundError = Error.NotFound(
        "ContentItemRevision.TranslationNotFound", "The requested language is not present in one of the compared snapshots.");

    public async Task<Result<CompareContentItemRevisionsResponse>> Handle(
        CompareContentItemRevisionsQuery request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.ContentItemId, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure<CompareContentItemRevisionsResponse>(
                Error.NotFound("ContentItem.NotFound", $"Content item '{request.ContentItemId}' could not be found."));
        }

        LanguageCode languageCode;
        if (!string.IsNullOrWhiteSpace(request.LanguageCode))
        {
            var languageCodeResult = LanguageCode.Create(request.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure<CompareContentItemRevisionsResponse>(languageCodeResult.Error);
            }

            languageCode = languageCodeResult.Value;
        }
        else
        {
            var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
            if (defaultLanguage is null)
            {
                return Result.Failure<CompareContentItemRevisionsResponse>(
                    Error.Failure("ContentItem.NoDefaultLanguage", "No default site language is configured."));
            }

            languageCode = defaultLanguage.Code;
        }

        var fromRevision = await contentItemRevisionRepository.GetByRevisionNumberAsync(request.ContentItemId, request.From, cancellationToken);
        if (fromRevision is null)
        {
            return Result.Failure<CompareContentItemRevisionsResponse>(Error.NotFound(
                "ContentItemRevision.NotFound", $"Revision '{request.From}' could not be found for this content item."));
        }

        var fromSnapshot = ContentItemRevisionSnapshotSerializer.Deserialize(fromRevision.SnapshotJson);
        var fromTranslation = fromSnapshot.Translations.FirstOrDefault(t => t.LanguageCode == languageCode.Value);
        if (fromTranslation is null)
        {
            return Result.Failure<CompareContentItemRevisionsResponse>(TranslationNotFoundError);
        }

        ContentItemRevisionSnapshotTranslation? toTranslation;
        IReadOnlyList<Guid> toCategoryIds;
        if (string.IsNullOrWhiteSpace(request.To) || string.Equals(request.To, "current", StringComparison.OrdinalIgnoreCase))
        {
            var currentSnapshot = ContentItemRevisionSnapshotSerializer.BuildFromContentItem(contentItem);
            toTranslation = currentSnapshot.Translations.FirstOrDefault(t => t.LanguageCode == languageCode.Value);
            toCategoryIds = currentSnapshot.CategoryIds;
        }
        else
        {
            if (!int.TryParse(request.To, out var toRevisionNumber))
            {
                return Result.Failure<CompareContentItemRevisionsResponse>(Error.Validation(
                    "ContentItemRevision.InvalidTo", "'to' must be a revision number or the literal 'current'."));
            }

            var toRevision = await contentItemRevisionRepository.GetByRevisionNumberAsync(request.ContentItemId, toRevisionNumber, cancellationToken);
            if (toRevision is null)
            {
                return Result.Failure<CompareContentItemRevisionsResponse>(Error.NotFound(
                    "ContentItemRevision.NotFound", $"Revision '{toRevisionNumber}' could not be found for this content item."));
            }

            var toSnapshot = ContentItemRevisionSnapshotSerializer.Deserialize(toRevision.SnapshotJson);
            toTranslation = toSnapshot.Translations.FirstOrDefault(t => t.LanguageCode == languageCode.Value);
            toCategoryIds = toSnapshot.CategoryIds;
        }

        if (toTranslation is null)
        {
            return Result.Failure<CompareContentItemRevisionsResponse>(TranslationNotFoundError);
        }

        var response = new CompareContentItemRevisionsResponse(
            languageCode.Value,
            Diff(fromTranslation.Title, toTranslation.Title),
            Diff(fromTranslation.Summary, toTranslation.Summary),
            Diff(fromTranslation.Body, toTranslation.Body),
            Diff(fromTranslation.MetaTitle, toTranslation.MetaTitle),
            Diff(fromTranslation.MetaDescription, toTranslation.MetaDescription),
            Diff(fromTranslation.OgTitle, toTranslation.OgTitle),
            Diff(fromTranslation.OgDescription, toTranslation.OgDescription),
            Diff(fromTranslation.NoIndex, toTranslation.NoIndex),
            Diff(fromTranslation.CanonicalUrl, toTranslation.CanonicalUrl),
            DiffIdList(fromTranslation.TagIds, toTranslation.TagIds),
            DiffIdList(fromSnapshot.CategoryIds, toCategoryIds));

        return Result.Success(response);
    }

    private static ContentItemRevisionFieldDiffResponse<T> Diff<T>(T from, T to) =>
        new(!Equals(from, to), from, to);

    private static ContentItemRevisionFieldDiffResponse<IReadOnlyList<Guid>> DiffIdList(IReadOnlyList<Guid> from, IReadOnlyList<Guid> to)
    {
        var fromSorted = from.OrderBy(id => id).ToList();
        var toSorted = to.OrderBy(id => id).ToList();
        return new ContentItemRevisionFieldDiffResponse<IReadOnlyList<Guid>>(!fromSorted.SequenceEqual(toSorted), fromSorted, toSorted);
    }
}
