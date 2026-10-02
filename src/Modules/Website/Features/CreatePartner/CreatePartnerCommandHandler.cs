using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreatePartner;

public sealed class CreatePartnerCommandHandler(
    IPartnerRepository partnerRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePartnerCommand, Result<CreatePartnerResponse>>
{
    public async Task<Result<CreatePartnerResponse>> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<CreatePartnerResponse>(
                Error.Failure("Partner.NoDefaultLanguage", "No default site language is configured."));
        }

        var logoCheck = await MediaImageReferenceGuard.CheckAsync(
            request.LogoMediaId, "Partner", "Logo", mediaAssetRepository, cancellationToken);
        if (logoCheck.IsFailure)
        {
            return Result.Failure<CreatePartnerResponse>(logoCheck.Error);
        }

        var partnerResult = Partner.Create(
            request.LogoMediaId, request.WebsiteUrl, request.SortOrder, defaultLanguage.Code,
            request.DefaultLanguageName, request.DefaultLanguageDescription,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (partnerResult.IsFailure)
        {
            return Result.Failure<CreatePartnerResponse>(partnerResult.Error);
        }

        partnerRepository.Add(partnerResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success(new CreatePartnerResponse(partnerResult.Value.Id, defaultLanguage.Code.Value));
    }
}
