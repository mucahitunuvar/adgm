using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeactivatePartner;

public sealed class DeactivatePartnerCommandHandler(
    IPartnerRepository partnerRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivatePartnerCommand, Result>
{
    public async Task<Result> Handle(DeactivatePartnerCommand request, CancellationToken cancellationToken)
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

        var deactivateResult = partner.Deactivate(currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
