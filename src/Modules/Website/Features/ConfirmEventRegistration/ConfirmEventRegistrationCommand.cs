using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ConfirmEventRegistration;

public sealed record ConfirmEventRegistrationCommand(Guid ContentItemId, Guid Id, byte[] RowVersion) : IRequest<Result>;
