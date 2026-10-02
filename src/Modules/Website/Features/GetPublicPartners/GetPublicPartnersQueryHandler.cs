using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Partners;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicPartners;

// ADR-024 §8.2 (Faz 2 Görev 3): the işbirlikleri page's partner list - active partners only, in
// SortOrder, and only those with a translation in the resolved language (mirrors
// GetPublicVideosQueryHandler's own language resolution).
public sealed class GetPublicPartnersQueryHandler(ISiteLanguageRepository siteLanguageRepository, PartnerPublicQueryService partnerPublicQueryService)
    : IRequestHandler<GetPublicPartnersQuery, Result<IReadOnlyList<PublicPartnerResponse>>>
{
    public async Task<Result<IReadOnlyList<PublicPartnerResponse>>> Handle(GetPublicPartnersQuery request, CancellationToken cancellationToken)
    {
        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);

        // Invariant guaranteed by SiteLanguage's own domain rules: the default language can never be
        // deactivated, so there is always exactly one active default to fall back to.
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);

        var partners = await partnerPublicQueryService.GetActiveAsync(resolvedLanguage.Code, cancellationToken);

        return Result.Success(partners);
    }
}
