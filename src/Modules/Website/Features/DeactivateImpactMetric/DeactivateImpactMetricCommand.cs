using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateImpactMetric;

public sealed record DeactivateImpactMetricCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
