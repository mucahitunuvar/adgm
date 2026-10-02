using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSliders;

public sealed record GetSlidersQuery : IRequest<Result<IReadOnlyList<SliderSummaryResponse>>>;
