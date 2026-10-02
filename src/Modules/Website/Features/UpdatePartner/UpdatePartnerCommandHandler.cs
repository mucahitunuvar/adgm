using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePartner;

public sealed class UpdatePartnerCommandHandler(
    IPartnerRepository partnerRepository,
    IMediaAssetRepository mediaAssetRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePartnerCommand, Result>
{
    public async Task<Result> Handle(UpdatePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = await partnerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (partner is null)
        {
            return Result.Failure(Error.NotFound("Partner.NotFound", $"Partner '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(partner.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "Partner.ConcurrencyConflict", "The partner was changed by someone else. Reload and try again."));
        }

        var logoCheck = await MediaImageReferenceGuard.CheckAsync(
            request.LogoMediaId, "Partner", "Logo", mediaAssetRepository, cancellationToken);
        if (logoCheck.IsFailure)
        {
            return logoCheck;
        }

        var updateResult = partner.Update(
            request.LogoMediaId, request.WebsiteUrl, request.SortOrder,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
