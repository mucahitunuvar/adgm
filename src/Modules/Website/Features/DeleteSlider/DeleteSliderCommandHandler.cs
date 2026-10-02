using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteSlider;

public sealed class DeleteSliderCommandHandler(
    ISliderRepository sliderRepository,
    ISliderUsageChecker sliderUsageChecker,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSliderCommand, Result>
{
    public async Task<Result> Handle(DeleteSliderCommand request, CancellationToken cancellationToken)
    {
        var slider = await sliderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (slider is null)
        {
            return Result.Failure(Error.NotFound("Slider.NotFound", $"Slider '{request.Id}' could not be found."));
        }

        var usages = await sliderUsageChecker.GetUsagesAsync(slider.Id, cancellationToken);
        if (usages.Count > 0)
        {
            var usageDescriptions = string.Join(", ", usages.Select(u => u.Description));
            return Result.Failure(Error.Conflict(
                "Slider.InUse", $"This slider is in use and cannot be deleted: {usageDescriptions}."));
        }

        sliderRepository.Remove(slider);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
