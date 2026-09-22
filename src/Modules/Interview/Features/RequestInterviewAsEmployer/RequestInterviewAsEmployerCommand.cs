using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsEmployer;

public sealed record RequestInterviewAsEmployerCommand(Guid CandidateCvId) : IRequest<Result<RequestInterviewAsEmployerResponse>>;
