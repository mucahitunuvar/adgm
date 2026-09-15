using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
