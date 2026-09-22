using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Interview.Features.RecordInterviewResult;

public sealed record RecordInterviewResultCommand(Guid InterviewId, string Outcome, string? ResultNotes) : IRequest<Result>;
