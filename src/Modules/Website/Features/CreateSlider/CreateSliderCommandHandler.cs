using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateSlider;

public sealed class CreateSliderCommandHandler(
    ISliderRepository sliderRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSliderCommand, Result<CreateSliderResponse>>
{
    public async Task<Result<CreateSliderResponse>> Handle(CreateSliderCommand request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<CreateSliderResponse>(Error.Failure("Slider.NoDefaultLanguage", "No default site language is configured."));
        }

        var keyResult = SliderKey.Create(request.Key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<CreateSliderResponse>(keyResult.Error);
        }

        var existing = await sliderRepository.GetByKeyAsync(keyResult.Value, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<CreateSliderResponse>(Error.Conflict(
                "Slider.KeyAlreadyExists", $"A slider with key '{keyResult.Value}' already exists."));
        }

        var sliderResult = Slider.Create(
            request.Key, defaultLanguage.Code, request.DefaultLanguageName,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (sliderResult.IsFailure)
        {
            return Result.Failure<CreateSliderResponse>(sliderResult.Error);
        }

        sliderRepository.Add(sliderResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success(new CreateSliderResponse(sliderResult.Value.Id, sliderResult.Value.Key.Value, defaultLanguage.Code.Value));
    }
}
