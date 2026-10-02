using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ActivatePartner;

public sealed class ActivatePartnerCommandHandler(
    IPartnerRepository partnerRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ActivatePartnerCommand, Result>
{
    public async Task<Result> Handle(ActivatePartnerCommand request, CancellationToken cancellationToken)
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

        var activateResult = partner.Activate(currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
