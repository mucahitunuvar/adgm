using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeletePartnerTranslation;

public sealed class DeletePartnerTranslationCommandHandler(
    IPartnerRepository partnerRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePartnerTranslationCommand, Result>
{
    public async Task<Result> Handle(DeletePartnerTranslationCommand request, CancellationToken cancellationToken)
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

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure(Error.Failure("Partner.NoDefaultLanguage", "No default site language is configured."));
        }

        var removeResult = partner.RemoveTranslation(
            languageCodeResult.Value, defaultLanguage.Code, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
