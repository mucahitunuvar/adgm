using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.ResetPassword;

public sealed record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Result>;
