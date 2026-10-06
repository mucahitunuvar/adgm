using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CancelEventRegistration;

public sealed record CancelEventRegistrationCommand(string? Token) : IRequest<Result>;
