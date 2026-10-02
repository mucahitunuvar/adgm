using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateImpactMetric;

public sealed record CreateImpactMetricCommand(
    decimal Value,
    string? IconKey,
    int SortOrder,
    string? DefaultLanguageLabel,
    string? DefaultLanguageUnit,
    string? DefaultLanguagePeriod,
    string? DefaultLanguageSource) : IRequest<Result<CreateImpactMetricResponse>>;
