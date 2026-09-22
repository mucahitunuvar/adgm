using GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsCandidate;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsEmployer;

public sealed record GetMyInterviewsAsEmployerQuery : IRequest<Result<IReadOnlyList<InterviewResponse>>>;
