using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;

public sealed class GetLegalDocumentByIdQueryHandler(ILegalDocumentRepository legalDocumentRepository, TimeProvider timeProvider)
    : IRequestHandler<GetLegalDocumentByIdQuery, Result<LegalDocumentDetailResponse>>
{
    public async Task<Result<LegalDocumentDetailResponse>> Handle(GetLegalDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await legalDocumentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (document is null)
        {
            return Result.Failure<LegalDocumentDetailResponse>(
                Error.NotFound("LegalDocument.NotFound", $"Legal document '{request.Id}' could not be found."));
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var effectiveVersion = LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, now);

        var translations = document.Translations
            .Select(t => new LegalDocumentTranslationResponse(t.LanguageCode.Value, t.Title))
            .ToList();

        var versions = document.Versions
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new LegalDocumentVersionResponse(
                v.Id,
                v.VersionNumber,
                v.Status.ToString(),
                v.EffectiveAtUtc,
                v.PublishedAtUtc,
                v.PublishedByUserId,
                v.ChangeSummary,
                v.Translations.Select(t => new LegalDocumentVersionTranslationResponse(t.LanguageCode.Value, t.Body)).ToList()))
            .ToList();

        var response = new LegalDocumentDetailResponse(
            document.Id, document.Key.Value, document.Kind.ToString(), document.RowVersion, document.CreatedAtUtc,
            effectiveVersion?.VersionNumber, translations, versions);

        return Result.Success(response);
    }
}
