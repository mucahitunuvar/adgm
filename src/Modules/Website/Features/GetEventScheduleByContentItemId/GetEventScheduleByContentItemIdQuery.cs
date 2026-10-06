using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetEventScheduleByContentItemId;

public sealed record GetEventScheduleByContentItemIdQuery(Guid ContentItemId) : IRequest<Result<EventScheduleDetailResponse>>;
