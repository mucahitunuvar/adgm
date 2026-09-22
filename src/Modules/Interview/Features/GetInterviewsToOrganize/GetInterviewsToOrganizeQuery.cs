using GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsCandidate;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Interview.Features.GetInterviewsToOrganize;

public sealed record GetInterviewsToOrganizeQuery : IRequest<Result<IReadOnlyList<InterviewResponse>>>;
