using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ChangeFormSubmissionStatus;

// Never invalidates WebsiteCacheInvalidator - submission management never changes a publicly cached
// response (see PublicContentCacheInvalidationTests.ExcludedHandlers), the same reasoning
// SubmitFormSubmissionCommandHandler itself documents.
public sealed class ChangeFormSubmissionStatusCommandHandler(
    IFormSubmissionRepository formSubmissionRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeFormSubmissionStatusCommand, Result>
{
    public async Task<Result> Handle(ChangeFormSubmissionStatusCommand request, CancellationToken cancellationToken)
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

        var status = Enum.Parse<FormSubmissionStatus>(request.Status);

        var changeResult = submission.ChangeStatus(status, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (changeResult.IsFailure)
        {
            return changeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
