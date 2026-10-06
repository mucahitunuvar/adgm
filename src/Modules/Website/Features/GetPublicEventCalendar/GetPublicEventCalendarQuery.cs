using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicEventCalendar;

public sealed record GetPublicEventCalendarQuery(Guid ContentItemId, string? Lang) : IRequest<Result<string>>;
