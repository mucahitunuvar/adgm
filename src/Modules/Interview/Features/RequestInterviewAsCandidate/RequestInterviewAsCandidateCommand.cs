using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsCandidate;

public sealed record RequestInterviewAsCandidateCommand(Guid CompanyId) : IRequest<Result<RequestInterviewAsCandidateResponse>>;
