using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateImpactMetric;

public sealed record UpdateImpactMetricCommand(Guid Id, byte[] RowVersion, decimal Value, string? IconKey, int SortOrder) : IRequest<Result>;
