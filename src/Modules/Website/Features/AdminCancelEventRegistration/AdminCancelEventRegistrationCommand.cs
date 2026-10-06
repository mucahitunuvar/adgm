using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.AdminCancelEventRegistration;

public sealed record AdminCancelEventRegistrationCommand(Guid ContentItemId, Guid Id, byte[] RowVersion) : IRequest<Result>;
