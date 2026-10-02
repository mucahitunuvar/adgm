using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSliders;

public sealed class GetSlidersQueryHandler(ISliderRepository sliderRepository, ISiteLanguageRepository siteLanguageRepository)
    : IRequestHandler<GetSlidersQuery, Result<IReadOnlyList<SliderSummaryResponse>>>
{
    public async Task<Result<IReadOnlyList<SliderSummaryResponse>>> Handle(GetSlidersQuery request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No default site language is configured.");

        var sliders = await sliderRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<SliderSummaryResponse> responses = sliders
            .Select(s => new SliderSummaryResponse(
                s.Id, s.Key.Value, ResolveName(s, defaultLanguage.Code), s.Slides.Count, s.RowVersion))
            .ToList();

        return Result.Success(responses);
    }

    private static string ResolveName(Slider slider, LanguageCode defaultLanguageCode) =>
        slider.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguageCode)?.Name ?? slider.Key.Value;
}
