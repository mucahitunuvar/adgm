using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.ConfirmMeeting;

public sealed record ConfirmMeetingCommand(Guid CandidateCvId, Guid MeetingRequestId) : IRequest<Result>;
