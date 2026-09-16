using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.ResendVerificationEmail;

public sealed record ResendVerificationEmailCommand(string Email) : IRequest<Result>;
