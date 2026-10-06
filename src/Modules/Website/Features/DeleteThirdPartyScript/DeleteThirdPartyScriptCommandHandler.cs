using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteThirdPartyScript;

// ADR-024 §13 (Faz 3 Görev 7): unlike Video/Slider, nothing references a ThirdPartyScript by id - it is
// only ever read as part of the active-script list the public site response builds - so there is no
// usage checker to consult here (mirrors DeletePartnerCommandHandler's own remarks).
public sealed class DeleteThirdPartyScriptCommandHandler(
    IThirdPartyScriptRepository thirdPartyScriptRepository,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteThirdPartyScriptCommand, Result>
{
    public async Task<Result> Handle(DeleteThirdPartyScriptCommand request, CancellationToken cancellationToken)
    {
        var script = await thirdPartyScriptRepository.GetByIdAsync(request.Id, cancellationToken);
        if (script is null)
        {
            return Result.Failure(Error.NotFound("ThirdPartyScript.NotFound", $"Third-party script '{request.Id}' could not be found."));
        }

        thirdPartyScriptRepository.Remove(script);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
