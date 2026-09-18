using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.ConfirmMeetingRequest;

// Endpoint'siz komut - yalnızca ICareerAdvisorModuleContract.ConfirmMeetingRequestAsync üzerinden,
// Candidate modülünün ConfirmMeetingCommand'ından ISender ile çağrılır (Görev 5/ADR-022 §4).
public sealed record ConfirmMeetingRequestCommand(Guid MeetingRequestId, Guid CandidateUserId) : IRequest<Result>;
