using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentRevisions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItemRevisionByNumber;

public sealed class GetContentItemRevisionByNumberQueryHandler(IContentItemRevisionRepository contentItemRevisionRepository)
    : IRequestHandler<GetContentItemRevisionByNumberQuery, Result<ContentItemRevisionDetailResponse>>
{
    public async Task<Result<ContentItemRevisionDetailResponse>> Handle(
        GetContentItemRevisionByNumberQuery request, CancellationToken cancellationToken)
    {
        var revision = await contentItemRevisionRepository.GetByRevisionNumberAsync(
            request.ContentItemId, request.RevisionNumber, cancellationToken);
        if (revision is null)
        {
            return Result.Failure<ContentItemRevisionDetailResponse>(Error.NotFound(
                "ContentItemRevision.NotFound", $"Revision '{request.RevisionNumber}' could not be found for this content item."));
        }

        var snapshot = ContentItemRevisionSnapshotSerializer.Deserialize(revision.SnapshotJson);
        var translations = snapshot.Translations
            .Select(t => new ContentItemRevisionTranslationSnapshotResponse(
                t.LanguageCode, t.Title, t.Summary, t.Body, t.MetaTitle, t.MetaDescription, t.OgTitle, t.OgDescription, t.NoIndex,
                t.CanonicalUrl, t.TagIds))
            .ToList();

        return Result.Success(new ContentItemRevisionDetailResponse(
            revision.RevisionNumber, revision.SavedAtUtc, revision.SavedByUserId, revision.Kind.ToString(), revision.ChangedLanguages,
            revision.IsPublishedSnapshot, translations, snapshot.CategoryIds));
    }
}
