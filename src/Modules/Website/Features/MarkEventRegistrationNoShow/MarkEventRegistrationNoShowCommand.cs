using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.MarkEventRegistrationNoShow;

public sealed record MarkEventRegistrationNoShowCommand(Guid ContentItemId, Guid Id, byte[] RowVersion) : IRequest<Result>;
