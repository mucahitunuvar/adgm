using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSliderById;

public sealed class GetSliderByIdQueryHandler(ISliderRepository sliderRepository)
    : IRequestHandler<GetSliderByIdQuery, Result<SliderDetailResponse>>
{
    public async Task<Result<SliderDetailResponse>> Handle(GetSliderByIdQuery request, CancellationToken cancellationToken)
    {
        var slider = await sliderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (slider is null)
        {
            return Result.Failure<SliderDetailResponse>(Error.NotFound("Slider.NotFound", $"Slider '{request.Id}' could not be found."));
        }

        return Result.Success(Map(slider));
    }

    internal static SliderDetailResponse Map(Slider slider)
    {
        var translations = slider.Translations
            .Select(t => new SliderTranslationResponse(t.LanguageCode.Value, t.Name))
            .ToList();

        var slides = slider.Slides
            .OrderBy(s => s.SortOrder)
            .Select(slide =>
            {
                SlideLinkResponse? link = slide.LinkTarget.IsEmpty
                    ? null
                    : new SlideLinkResponse(
                        slide.LinkTarget.Kind.ToString(), slide.LinkTarget.ContentItemId, slide.LinkTarget.ContentTypeId,
                        slide.LinkTarget.InternalPath, slide.LinkTarget.ExternalUrl);

                var slideTranslations = slide.Translations
                    .Select(t => new SlideTranslationResponse(t.LanguageCode.Value, t.Eyebrow, t.Title, t.Text, t.ButtonLabel, t.AltTextOverride))
                    .ToList();

                return new SlideResponse(
                    slide.Id, slide.DesktopImageMediaId, slide.MobileImageMediaId, link, slide.SortOrder, slide.IsActive,
                    slide.PublishAtUtc, slide.UnpublishAtUtc, slideTranslations);
            })
            .ToList();

        return new SliderDetailResponse(slider.Id, slider.Key.Value, slider.RowVersion, translations, slides);
    }
}
