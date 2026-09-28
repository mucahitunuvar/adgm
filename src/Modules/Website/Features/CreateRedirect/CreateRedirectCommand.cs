using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateRedirect;

public sealed record CreateRedirectCommand(
    string? LanguageCode, string? FromPath, string? TargetKind, Guid? TargetContentItemId, string? TargetPath, string? StatusCode)
    : IRequest<Result<CreateRedirectResponse>>;
