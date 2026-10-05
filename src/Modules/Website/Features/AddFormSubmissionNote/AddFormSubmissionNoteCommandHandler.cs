using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.AddFormSubmissionNote;

public sealed class AddFormSubmissionNoteCommandHandler(
    IFormSubmissionRepository formSubmissionRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AddFormSubmissionNoteCommand, Result>
{
    public async Task<Result> Handle(AddFormSubmissionNoteCommand request, CancellationToken cancellationToken)
    {
        var submission = await formSubmissionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (submission is null)
        {
            return Result.Failure(Error.NotFound("FormSubmission.NotFound", $"Submission '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(submission.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "FormSubmission.ConcurrencyConflict", "The submission was changed by someone else. Reload and try again."));
        }

        var noteResult = submission.AddInternalNote(currentUserContext.UserId!.Value, request.Text, timeProvider.GetUtcNow().UtcDateTime);
        if (noteResult.IsFailure)
        {
            return noteResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
