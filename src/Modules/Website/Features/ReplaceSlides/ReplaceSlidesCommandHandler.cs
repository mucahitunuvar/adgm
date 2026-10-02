using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceSlides;

// Faz 2 Görev 2 master prompt §2: replaces a Slider's entire slide list in one call - the admin slide
// editor always saves every slide together, the same "whole collection replaced at once" shape
// ReplaceMenuItemsCommandHandler uses for a Menu's item tree. Slide has no parent/child relationships
// of its own, so unlike MenuItem there is no temp-id resolution: every slide gets a fresh Slide.Create
// on every save (Slide.Id is never referenced from outside this Slider).
public sealed class ReplaceSlidesCommandHandler(
    ISliderRepository sliderRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ReplaceSlidesCommand, Result>
{
    public async Task<Result> Handle(ReplaceSlidesCommand request, CancellationToken cancellationToken)
    {
        var slider = await sliderRepository.GetByIdAsync(request.SliderId, cancellationToken);
        if (slider is null)
        {
            return Result.Failure(Error.NotFound("Slider.NotFound", $"Slider '{request.SliderId}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(slider.RowVersion))
        {
            return Result.Failure(Error.Conflict("Slider.ConcurrencyConflict", "The slider was changed by someone else. Reload and try again."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No default site language is configured.");

        var slides = new List<Slide>();
        foreach (var input in request.Slides)
        {
            var buildResult = await BuildSlideAsync(input, defaultLanguage.Code, cancellationToken);
            if (buildResult.IsFailure)
            {
                return Result.Failure(buildResult.Error);
            }

            slides.Add(buildResult.Value);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var replaceResult = slider.ReplaceSlides(slides, currentUserContext.UserId!.Value, now);
        if (replaceResult.IsFailure)
        {
            return replaceResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }

    private async Task<Result<Slide>> BuildSlideAsync(SlideInput input, LanguageCode defaultLanguageCode, CancellationToken cancellationToken)
    {
        var desktopImageCheck = await MediaImageReferenceGuard.CheckAsync(
            input.DesktopImageMediaId, "Slide", "DesktopImage", mediaAssetRepository, cancellationToken);
        if (desktopImageCheck.IsFailure)
        {
            return Result.Failure<Slide>(desktopImageCheck.Error);
        }

        var mobileImageCheck = await MediaImageReferenceGuard.CheckAsync(
            input.MobileImageMediaId, "Slide", "MobileImage", mediaAssetRepository, cancellationToken);
        if (mobileImageCheck.IsFailure)
        {
            return Result.Failure<Slide>(mobileImageCheck.Error);
        }

        var linkTargetResult = BuildLinkTarget(input.Link);
        if (linkTargetResult.IsFailure)
        {
            return Result.Failure<Slide>(linkTargetResult.Error);
        }

        var translations = new List<SlideTranslation>();
        foreach (var translationInput in input.Translations)
        {
            var languageCodeResult = LanguageCode.Create(translationInput.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure<Slide>(languageCodeResult.Error);
            }

            var translationResult = SlideTranslation.Create(
                languageCodeResult.Value, translationInput.Eyebrow, translationInput.Title, translationInput.Text,
                translationInput.ButtonLabel, translationInput.AltTextOverride);
            if (translationResult.IsFailure)
            {
                return Result.Failure<Slide>(translationResult.Error);
            }

            translations.Add(translationResult.Value);
        }

        // Faz 2 master prompt §1 "Çeviri kuralı": the default language's translation is required.
        if (translations.All(t => t.LanguageCode != defaultLanguageCode))
        {
            return Result.Failure<Slide>(Error.Validation(
                "Slide.DefaultLanguageTranslationRequired", $"Each slide must have a translation in the default language '{defaultLanguageCode}'."));
        }

        return Slide.Create(
            input.DesktopImageMediaId, input.MobileImageMediaId, linkTargetResult.Value, input.SortOrder, input.IsActive,
            input.PublishAtUtc, input.UnpublishAtUtc, translations);
    }

    private static Result<LinkTarget> BuildLinkTarget(SlideLinkInput? link)
    {
        if (link is null)
        {
            return LinkTarget.CreateEmpty();
        }

        if (!Enum.TryParse<LinkTargetKind>(link.Kind, ignoreCase: true, out var kind))
        {
            return Result.Failure<LinkTarget>(Error.Validation("Slide.LinkKindInvalid", $"Unknown link kind '{link.Kind}'."));
        }

        return kind switch
        {
            LinkTargetKind.None => LinkTarget.CreateEmpty(),
            LinkTargetKind.Content => LinkTarget.ForContent(link.ContentItemId ?? Guid.Empty),
            LinkTargetKind.ContentTypeListing => LinkTarget.ForContentTypeListing(link.ContentTypeId ?? Guid.Empty),
            LinkTargetKind.InternalPath => LinkTarget.ForInternalPath(link.InternalPath),
            LinkTargetKind.ExternalUrl => LinkTarget.ForExternalUrl(link.ExternalUrl),
            _ => Result.Failure<LinkTarget>(Error.Validation("Slide.LinkKindInvalid", $"Unknown link kind '{link.Kind}'.")),
        };
    }
}
