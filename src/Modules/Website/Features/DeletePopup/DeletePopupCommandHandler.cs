using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeletePopup;

// ADR-024 §9 (Faz 2 Görev 6): nothing references a Popup by id - frontend display decisions read every
// currently-visible popup from the public site response, never a specific one - so there is no usage
// checker to consult here (only the Modal image MediaAsset is protected, by PopupMediaUsageProvider).
public sealed class DeletePopupCommandHandler(
    IPopupRepository popupRepository,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePopupCommand, Result>
{
    public async Task<Result> Handle(DeletePopupCommand request, CancellationToken cancellationToken)
    {
        var popup = await popupRepository.GetByIdAsync(request.Id, cancellationToken);
        if (popup is null)
        {
            return Result.Failure(Error.NotFound("Popup.NotFound", $"Popup '{request.Id}' could not be found."));
        }

        popupRepository.Remove(popup);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
