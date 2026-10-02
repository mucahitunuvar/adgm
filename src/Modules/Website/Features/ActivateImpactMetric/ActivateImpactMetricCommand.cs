using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ActivateImpactMetric;

public sealed record ActivateImpactMetricCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
