using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.RequestMeeting;

public sealed record RequestMeetingCommand(Guid CandidateCvId) : IRequest<Result<RequestMeetingResponse>>;
