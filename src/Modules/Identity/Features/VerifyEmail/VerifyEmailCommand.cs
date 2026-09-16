using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.VerifyEmail;

public sealed record VerifyEmailCommand(string Token) : IRequest<Result>;
