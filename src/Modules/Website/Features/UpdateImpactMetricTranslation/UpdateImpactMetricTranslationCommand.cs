using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateImpactMetricTranslation;

public sealed record UpdateImpactMetricTranslationCommand(
    Guid Id, string LanguageCode, byte[] RowVersion, string? Label, string? Unit, string? Period, string? Source) : IRequest<Result>;
