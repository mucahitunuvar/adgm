using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ScheduleContentItem;

public sealed record ScheduleContentItemCommand(Guid Id, byte[] RowVersion, DateTime? PublishAtUtc, DateTime? UnpublishAtUtc) : IRequest<Result>;
