using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.RejectMeetingRequest;

public sealed record RejectMeetingRequestCommand(Guid MeetingRequestId) : IRequest<Result>;
