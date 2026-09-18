using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.RequestMeeting;

// Görev 5/ADR-022 §4: "Aday talep açar (mevcut Candidate → CareerAdvisor yönünü kullanır)" - bu
// komut Candidate modülünde yaşıyor, MeetingRequest aggregate'ini ICareerAdvisorModuleContract
// üzerinden CareerAdvisor modülünde oluşturtuyor.
public sealed class RequestMeetingCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICurrentUserContext currentUserContext,
    ICareerAdvisorModuleContract careerAdvisorModuleContract)
    : IRequestHandler<RequestMeetingCommand, Result<RequestMeetingResponse>>
{
    public async Task<Result<RequestMeetingResponse>> Handle(RequestMeetingCommand request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure<RequestMeetingResponse>(
                Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        if (candidateCv.UserId != currentUserContext.UserId)
        {
            return Result.Failure<RequestMeetingResponse>(
                Error.Forbidden("CandidateCv.NotOwner", "You may only request a meeting for your own candidate CV."));
        }

        if (candidateCv.CareerAdvisorId is null)
        {
            return Result.Failure<RequestMeetingResponse>(
                Error.Conflict("CandidateCv.NoAssignedAdvisor", "You do not have an assigned career advisor yet."));
        }

        var createResult = await careerAdvisorModuleContract.CreateMeetingRequestAsync(
            candidateCv.Id, candidateCv.UserId, candidateCv.CareerAdvisorId.Value, cancellationToken);

        return createResult.IsFailure
            ? Result.Failure<RequestMeetingResponse>(createResult.Error)
            : Result.Success(new RequestMeetingResponse(createResult.Value));
    }
}
