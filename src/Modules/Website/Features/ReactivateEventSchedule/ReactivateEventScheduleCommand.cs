using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ReactivateEventSchedule;

public sealed record ReactivateEventScheduleCommand(Guid ContentItemId, byte[] RowVersion) : IRequest<Result>;
