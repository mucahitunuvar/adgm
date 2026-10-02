using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteImpactMetricTranslation;

public sealed record DeleteImpactMetricTranslationCommand(Guid Id, string LanguageCode, byte[] RowVersion) : IRequest<Result>;
