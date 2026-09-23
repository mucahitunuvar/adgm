using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.SetDefaultSiteLanguage;

public sealed class SetDefaultSiteLanguageCommandHandler(
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SetDefaultSiteLanguageCommand, Result>
{
    public async Task<Result> Handle(SetDefaultSiteLanguageCommand request, CancellationToken cancellationToken)
    {
        var target = await siteLanguageRepository.GetByIdAsync(request.Id, cancellationToken);
        if (target is null)
        {
            return Result.Failure(Error.NotFound("SiteLanguage.NotFound", "The specified site language could not be found."));
        }

        if (target.IsDefault)
        {
            return Result.Success();
        }

        var userId = currentUserContext.UserId!.Value;
        var updatedAtUtc = DateTime.UtcNow;

        var markResult = target.MarkAsDefault(userId, updatedAtUtc);
        if (markResult.IsFailure)
        {
            return markResult;
        }

        // ADR-024 §3: default swap is atomic - both mutations land in the same DbContext instance
        // and are committed by the single SaveChangesAsync call below.
        var currentDefault = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (currentDefault is not null && currentDefault.Id != target.Id)
        {
            currentDefault.UnmarkAsDefault(userId, updatedAtUtc);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
