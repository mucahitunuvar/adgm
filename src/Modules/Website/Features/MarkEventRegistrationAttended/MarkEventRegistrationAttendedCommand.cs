using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.MarkEventRegistrationAttended;

public sealed record MarkEventRegistrationAttendedCommand(Guid ContentItemId, Guid Id, byte[] RowVersion) : IRequest<Result>;
