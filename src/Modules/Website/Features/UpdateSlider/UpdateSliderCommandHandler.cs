using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSlider;

public sealed class UpdateSliderCommandHandler(
    ISliderRepository sliderRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSliderCommand, Result>
{
    public async Task<Result> Handle(UpdateSliderCommand request, CancellationToken cancellationToken)
    {
        var slider = await sliderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (slider is null)
        {
            return Result.Failure(Error.NotFound("Slider.NotFound", $"Slider '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(slider.RowVersion))
        {
            return Result.Failure(Error.Conflict("Slider.ConcurrencyConflict", "The slider was changed by someone else. Reload and try again."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No default site language is configured.");

        var translations = new List<SliderTranslation>();
        foreach (var input in request.Translations)
        {
            var languageCodeResult = LanguageCode.Create(input.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure(languageCodeResult.Error);
            }

            var translationResult = SliderTranslation.Create(languageCodeResult.Value, input.Name);
            if (translationResult.IsFailure)
            {
                return Result.Failure(translationResult.Error);
            }

            translations.Add(translationResult.Value);
        }

        if (translations.All(t => t.LanguageCode != defaultLanguage.Code))
        {
            return Result.Failure(Error.Validation(
                "Slider.DefaultLanguageTranslationRequired", $"A translation in the default language '{defaultLanguage.Code}' is required."));
        }

        var replaceResult = slider.ReplaceTranslations(translations, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (replaceResult.IsFailure)
        {
            return replaceResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
