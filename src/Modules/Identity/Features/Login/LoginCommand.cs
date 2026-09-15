using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;
