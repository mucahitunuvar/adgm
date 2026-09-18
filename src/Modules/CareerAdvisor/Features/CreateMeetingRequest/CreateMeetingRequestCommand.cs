using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.CreateMeetingRequest;

// Endpoint'siz komut (Identity'nin CreateStaffUserCommand deseni) - yalnızca
// ICareerAdvisorModuleContract.CreateMeetingRequestAsync üzerinden, Candidate modülünün
// RequestMeetingCommand'ından ISender ile çağrılır (Görev 5/ADR-022 §4).
public sealed record CreateMeetingRequestCommand(Guid CandidateCvId, Guid CandidateUserId, Guid CareerAdvisorId)
    : IRequest<Result<Guid>>;
