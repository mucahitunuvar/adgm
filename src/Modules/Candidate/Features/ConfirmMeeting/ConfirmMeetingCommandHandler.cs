using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.ConfirmMeeting;

public sealed class ConfirmMeetingCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICurrentUserContext currentUserContext,
    ICareerAdvisorModuleContract careerAdvisorModuleContract)
    : IRequestHandler<ConfirmMeetingCommand, Result>
{
    public async Task<Result> Handle(ConfirmMeetingCommand request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure(Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        if (candidateCv.UserId != currentUserContext.UserId)
        {
            return Result.Failure(
                Error.Forbidden("CandidateCv.NotOwner", "You may only confirm a meeting for your own candidate CV."));
        }

        return await careerAdvisorModuleContract.ConfirmMeetingRequestAsync(
            request.MeetingRequestId, candidateCv.UserId, cancellationToken);
    }
}
