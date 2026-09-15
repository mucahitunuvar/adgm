using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest<Result>;
