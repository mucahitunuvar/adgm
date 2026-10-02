using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSliderById;

public sealed record GetSliderByIdQuery(Guid Id) : IRequest<Result<SliderDetailResponse>>;
