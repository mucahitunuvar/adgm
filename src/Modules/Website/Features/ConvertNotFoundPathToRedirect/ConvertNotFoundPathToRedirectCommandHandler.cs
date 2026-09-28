using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ConvertNotFoundPathToRedirect;

// ADR-024 §15 (Faz 1a Görev 5): one transaction creates the Redirect and removes the NotFoundLog row -
// either both happen or neither does (a single UnitOfWork.SaveChangesAsync commits both changes
// together).
public sealed class ConvertNotFoundPathToRedirectCommandHandler(
    INotFoundLogRepository notFoundLogRepository,
    IRedirectRepository redirectRepository,
    IContentItemRepository contentItemRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ConvertNotFoundPathToRedirectCommand, Result<ConvertNotFoundPathToRedirectResponse>>
{
    public async Task<Result<ConvertNotFoundPathToRedirectResponse>> Handle(
        ConvertNotFoundPathToRedirectCommand request, CancellationToken cancellationToken)
    {
        var notFoundLog = await notFoundLogRepository.GetByIdAsync(request.NotFoundLogId, cancellationToken);
        if (notFoundLog is null)
        {
            return Result.Failure<ConvertNotFoundPathToRedirectResponse>(
                Error.NotFound("NotFoundLog.NotFound", $"Not-found path log '{request.NotFoundLogId}' could not be found."));
        }

        if (!Enum.TryParse<RedirectTargetKind>(request.TargetKind, ignoreCase: true, out var targetKind))
        {
            return Result.Failure<ConvertNotFoundPathToRedirectResponse>(Error.Validation(
                "Redirect.InvalidTargetKind", $"'{request.TargetKind}' is not a recognized target kind."));
        }

        if (!Enum.TryParse<RedirectStatusCode>(request.StatusCode, ignoreCase: true, out var statusCode))
        {
            return Result.Failure<ConvertNotFoundPathToRedirectResponse>(Error.Validation(
                "Redirect.InvalidStatusCode", $"'{request.StatusCode}' is not a recognized status code."));
        }

        if (targetKind == RedirectTargetKind.ContentItem)
        {
            if (request.TargetContentItemId is null
                || await contentItemRepository.GetByIdAsync(request.TargetContentItemId.Value, cancellationToken) is null)
            {
                return Result.Failure<ConvertNotFoundPathToRedirectResponse>(Error.NotFound(
                    "Redirect.TargetContentItemNotFound", $"Content item '{request.TargetContentItemId}' could not be found."));
            }
        }

        var redirectResult = Redirect.Create(
            notFoundLog.LanguageCode, notFoundLog.Path, targetKind, request.TargetContentItemId, request.TargetPath, statusCode,
            currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (redirectResult.IsFailure)
        {
            return Result.Failure<ConvertNotFoundPathToRedirectResponse>(redirectResult.Error);
        }

        var redirect = redirectResult.Value;

        var collisionCheck = await RedirectCollisionGuard.CheckAsync(redirect, contentItemRepository, redirectRepository, cancellationToken);
        if (collisionCheck.IsFailure)
        {
            return Result.Failure<ConvertNotFoundPathToRedirectResponse>(collisionCheck.Error);
        }

        redirectRepository.Add(redirect);
        notFoundLogRepository.Remove(notFoundLog);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ConvertNotFoundPathToRedirectResponse(redirect.Id, redirect.FromPath));
    }
}
