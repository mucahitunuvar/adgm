using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Interview.Features.CancelInterview;

public sealed record CancelInterviewCommand(Guid InterviewId) : IRequest<Result>;
