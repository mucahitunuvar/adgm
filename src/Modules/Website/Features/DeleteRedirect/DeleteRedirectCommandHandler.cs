using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteRedirect;

// Both automatic and manual redirects can be deleted (ADR-024 §15) - only editing (UpdateRedirect) is
// restricted to manual ones.
public sealed class DeleteRedirectCommandHandler(
    IRedirectRepository redirectRepository,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteRedirectCommand, Result>
{
    public async Task<Result> Handle(DeleteRedirectCommand request, CancellationToken cancellationToken)
    {
        var redirect = await redirectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (redirect is null)
        {
            return Result.Failure(Error.NotFound("Redirect.NotFound", $"Redirect '{request.Id}' could not be found."));
        }

        redirectRepository.Remove(redirect);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicContent(cacheService);

        return Result.Success();
    }
}
