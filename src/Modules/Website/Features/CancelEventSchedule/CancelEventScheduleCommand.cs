using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CancelEventSchedule;

public sealed record CancelEventScheduleCommand(Guid ContentItemId, byte[] RowVersion, string? Reason) : IRequest<Result>;
