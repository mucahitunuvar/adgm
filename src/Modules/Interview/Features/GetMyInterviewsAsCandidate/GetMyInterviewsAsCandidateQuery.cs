using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsCandidate;

public sealed record GetMyInterviewsAsCandidateQuery : IRequest<Result<IReadOnlyList<InterviewResponse>>>;
