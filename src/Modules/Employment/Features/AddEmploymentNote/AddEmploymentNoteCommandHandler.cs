using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Employment.Application.Abstractions;
using GenclikMerkezi.Modules.Employment.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employment.Features.AddEmploymentNote;

// EndEmploymentCommandHandler'daki taze-danışman kontrolünün aynısı.
public sealed class AddEmploymentNoteCommandHandler(
    IEmploymentRepository employmentRepository,
    IEmploymentNoteRepository employmentNoteRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmploymentModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AddEmploymentNoteCommand, Result<AddEmploymentNoteResponse>>
{
    public async Task<Result<AddEmploymentNoteResponse>> Handle(AddEmploymentNoteCommand request, CancellationToken cancellationToken)
    {
        var employment = await employmentRepository.GetByIdAsync(request.EmploymentId, cancellationToken);

        if (employment is null)
        {
            return Result.Failure<AddEmploymentNoteResponse>(
                Error.NotFound("Employment.NotFound", "The specified employment record could not be found."));
        }

        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        var currentAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(
            employment.CandidateCvId, cancellationToken);

        if (callerAdvisorId is null || callerAdvisorId != currentAdvisorId)
        {
            return Result.Failure<AddEmploymentNoteResponse>(Error.Forbidden(
                "Employment.NotCurrentAdvisor", "Only the candidate's current career advisor may add a note to this employment."));
        }

        var note = EmploymentNote.Create(employment.Id, callerAdvisorId.Value, request.Content, DateTime.UtcNow);
        employmentNoteRepository.Add(note);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new AddEmploymentNoteResponse(note.Id));
    }
}
