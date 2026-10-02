using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetImpactMetricById;

public sealed record GetImpactMetricByIdQuery(Guid Id) : IRequest<Result<ImpactMetricDetailResponse>>;
