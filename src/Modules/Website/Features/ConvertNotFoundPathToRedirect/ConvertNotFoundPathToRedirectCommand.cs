using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ConvertNotFoundPathToRedirect;

public sealed record ConvertNotFoundPathToRedirectCommand(Guid NotFoundLogId, string? TargetKind, Guid? TargetContentItemId, string? TargetPath, string? StatusCode)
    : IRequest<Result<ConvertNotFoundPathToRedirectResponse>>;
