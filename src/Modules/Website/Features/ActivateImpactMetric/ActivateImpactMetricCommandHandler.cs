using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ActivateImpactMetric;

public sealed class ActivateImpactMetricCommandHandler(
    IImpactMetricRepository impactMetricRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateImpactMetricCommand, Result>
{
    public async Task<Result> Handle(ActivateImpactMetricCommand request, CancellationToken cancellationToken)
    {
        var metric = await impactMetricRepository.GetByIdAsync(request.Id, cancellationToken);
        if (metric is null)
        {
            return Result.Failure(Error.NotFound("ImpactMetric.NotFound", $"Impact metric '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(metric.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "ImpactMetric.ConcurrencyConflict", "The impact metric was changed by someone else. Reload and try again."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure(Error.Failure("ImpactMetric.NoDefaultLanguage", "No default site language is configured."));
        }

        var activateResult = metric.Activate(
            defaultLanguage.Code, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
