using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeletePartner;

// ADR-024 §8.2 (Faz 2 Görev 3): unlike Video/Slider, nothing references a Partner by id yet - the
// logo-strip block (Görev 5) always shows every active partner, it never targets a specific one - so
// there is no usage checker to consult here (only the logo MediaAsset is protected, by
// PartnerMediaUsageProvider).
public sealed class DeletePartnerCommandHandler(
    IPartnerRepository partnerRepository,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePartnerCommand, Result>
{
    public async Task<Result> Handle(DeletePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = await partnerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (partner is null)
        {
            return Result.Failure(Error.NotFound("Partner.NotFound", $"Partner '{request.Id}' could not be found."));
        }

        partnerRepository.Remove(partner);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
