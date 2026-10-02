using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateImpactMetricTranslation;

public sealed class UpdateImpactMetricTranslationCommandHandler(
    IImpactMetricRepository impactMetricRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateImpactMetricTranslationCommand, Result>
{
    public async Task<Result> Handle(UpdateImpactMetricTranslationCommand request, CancellationToken cancellationToken)
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

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure(Error.Failure("ImpactMetric.NoDefaultLanguage", "No default site language is configured."));
        }

        var setResult = metric.SetTranslation(
            languageCodeResult.Value, defaultLanguage.Code, request.Label, request.Unit, request.Period, request.Source,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
