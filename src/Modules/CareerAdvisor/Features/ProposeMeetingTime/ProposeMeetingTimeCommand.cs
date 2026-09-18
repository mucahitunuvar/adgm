using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.ProposeMeetingTime;

public sealed record ProposeMeetingTimeCommand(Guid MeetingRequestId, DateTime ProposedDateTimeUtc) : IRequest<Result>;
