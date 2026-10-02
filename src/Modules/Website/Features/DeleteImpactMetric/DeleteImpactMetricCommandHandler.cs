using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteImpactMetric;

// ADR-024 §8.2 (Faz 2 Görev 3): a layout usage checker is introduced in Görev 4, once the
// impact-stats block can reference a metric by id (LayoutMediaUsageProvider's remarks) - there is
// nothing to consult yet, so delete is unconditional here, like Partner's.
public sealed class DeleteImpactMetricCommandHandler(
    IImpactMetricRepository impactMetricRepository,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteImpactMetricCommand, Result>
{
    public async Task<Result> Handle(DeleteImpactMetricCommand request, CancellationToken cancellationToken)
    {
        var metric = await impactMetricRepository.GetByIdAsync(request.Id, cancellationToken);
        if (metric is null)
        {
            return Result.Failure(Error.NotFound("ImpactMetric.NotFound", $"Impact metric '{request.Id}' could not be found."));
        }

        impactMetricRepository.Remove(metric);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
