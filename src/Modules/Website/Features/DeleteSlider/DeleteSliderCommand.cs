using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteSlider;

public sealed record DeleteSliderCommand(Guid Id) : IRequest<Result>;
