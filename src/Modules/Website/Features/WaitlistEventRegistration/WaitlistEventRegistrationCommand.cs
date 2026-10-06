using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.WaitlistEventRegistration;

public sealed record WaitlistEventRegistrationCommand(Guid ContentItemId, Guid Id, byte[] RowVersion) : IRequest<Result>;
