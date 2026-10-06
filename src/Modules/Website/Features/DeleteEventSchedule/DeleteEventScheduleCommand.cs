using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteEventSchedule;

public sealed record DeleteEventScheduleCommand(Guid ContentItemId) : IRequest<Result>;
