using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSlider;

public sealed record UpdateSliderCommand(Guid Id, byte[] RowVersion, IReadOnlyList<SliderTranslationInput> Translations) : IRequest<Result>;
