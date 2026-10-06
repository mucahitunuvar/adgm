using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.RejectEventRegistration;

public sealed record RejectEventRegistrationCommand(Guid ContentItemId, Guid Id, byte[] RowVersion, string? Reason) : IRequest<Result>;
