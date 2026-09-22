using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Interview.Features.ScheduleInterview;

public sealed record ScheduleInterviewCommand(Guid InterviewId, DateTime ScheduledAtUtc) : IRequest<Result>;
