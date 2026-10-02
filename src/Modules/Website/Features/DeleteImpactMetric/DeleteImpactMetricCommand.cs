using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteImpactMetric;

public sealed record DeleteImpactMetricCommand(Guid Id) : IRequest<Result>;
