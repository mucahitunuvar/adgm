using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePartnerTranslation;

public sealed class UpdatePartnerTranslationCommandHandler(
    IPartnerRepository partnerRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePartnerTranslationCommand, Result>
{
    public async Task<Result> Handle(UpdatePartnerTranslationCommand request, CancellationToken cancellationToken)
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

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var setResult = partner.SetTranslation(
            languageCodeResult.Value, request.Name, request.Description,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
