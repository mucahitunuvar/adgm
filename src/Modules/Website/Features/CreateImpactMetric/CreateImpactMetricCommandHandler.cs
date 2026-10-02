using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateImpactMetric;

public sealed class CreateImpactMetricCommandHandler(
    IImpactMetricRepository impactMetricRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateImpactMetricCommand, Result<CreateImpactMetricResponse>>
{
    public async Task<Result<CreateImpactMetricResponse>> Handle(CreateImpactMetricCommand request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<CreateImpactMetricResponse>(
                Error.Failure("ImpactMetric.NoDefaultLanguage", "No default site language is configured."));
        }

        var metricResult = ImpactMetric.Create(
            request.Value, request.IconKey, request.SortOrder, defaultLanguage.Code,
            request.DefaultLanguageLabel, request.DefaultLanguageUnit, request.DefaultLanguagePeriod, request.DefaultLanguageSource,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (metricResult.IsFailure)
        {
            return Result.Failure<CreateImpactMetricResponse>(metricResult.Error);
        }

        impactMetricRepository.Add(metricResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success(new CreateImpactMetricResponse(metricResult.Value.Id, defaultLanguage.Code.Value));
    }
}
