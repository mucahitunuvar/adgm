using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateImpactMetric;

public sealed class UpdateImpactMetricCommandHandler(
    IImpactMetricRepository impactMetricRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateImpactMetricCommand, Result>
{
    public async Task<Result> Handle(UpdateImpactMetricCommand request, CancellationToken cancellationToken)
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

        var updateResult = metric.Update(
            request.Value, request.IconKey, request.SortOrder, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
