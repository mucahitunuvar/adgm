using GenclikMerkezi.Modules.Interview.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Interview.Features.RecordInterviewResult;

public sealed record RecordInterviewResultCommand(Guid InterviewId, InterviewResult Outcome, string? ResultNotes) : IRequest<Result>;
