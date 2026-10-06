using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.VerifyEventRegistration;

public sealed record VerifyEventRegistrationCommand(string? Token) : IRequest<Result>;
