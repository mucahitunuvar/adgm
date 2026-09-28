using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateRedirect;

public sealed record UpdateRedirectCommand(Guid Id, string? TargetKind, Guid? TargetContentItemId, string? TargetPath, string? StatusCode)
    : IRequest<Result>;
