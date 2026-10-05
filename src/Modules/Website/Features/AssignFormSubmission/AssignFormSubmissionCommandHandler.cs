using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.AssignFormSubmission;

public sealed class AssignFormSubmissionCommandHandler(
    IFormSubmissionRepository formSubmissionRepository,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AssignFormSubmissionCommand, Result>
{
    public async Task<Result> Handle(AssignFormSubmissionCommand request, CancellationToken cancellationToken)
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

        var assignResult = submission.AssignTo(request.AssignedToUserId);
        if (assignResult.IsFailure)
        {
            return assignResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
