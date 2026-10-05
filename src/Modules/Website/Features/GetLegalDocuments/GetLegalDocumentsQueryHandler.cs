using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetLegalDocuments;

public sealed class GetLegalDocumentsQueryHandler(
    ILegalDocumentRepository legalDocumentRepository, ISiteLanguageRepository siteLanguageRepository, TimeProvider timeProvider)
    : IRequestHandler<GetLegalDocumentsQuery, Result<IReadOnlyList<LegalDocumentSummaryResponse>>>
{
    public async Task<Result<IReadOnlyList<LegalDocumentSummaryResponse>>> Handle(
        GetLegalDocumentsQuery request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<IReadOnlyList<LegalDocumentSummaryResponse>>(
                Error.Failure("LegalDocument.NoDefaultLanguage", "No default site language is configured."));
        }

        var documents = await legalDocumentRepository.GetAllAsync(cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        IReadOnlyList<LegalDocumentSummaryResponse> responses = documents
            .Select(d => new LegalDocumentSummaryResponse(
                d.Id,
                d.Key.Value,
                d.Kind.ToString(),
                ResolveTitle(d, defaultLanguage.Code),
                LegalDocumentEffectiveVersionResolver.Resolve(d.Versions, now)?.VersionNumber,
                d.HasDraft,
                d.RowVersion))
            .ToList();

        return Result.Success(responses);
    }

    private static string ResolveTitle(LegalDocument document, LanguageCode defaultLanguageCode) =>
        document.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguageCode)?.Title ?? document.Key.Value;
}
