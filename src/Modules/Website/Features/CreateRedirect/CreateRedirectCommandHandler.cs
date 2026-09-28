using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateRedirect;

public sealed class CreateRedirectCommandHandler(
    IRedirectRepository redirectRepository,
    IContentItemRepository contentItemRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateRedirectCommand, Result<CreateRedirectResponse>>
{
    public async Task<Result<CreateRedirectResponse>> Handle(CreateRedirectCommand request, CancellationToken cancellationToken)
    {
        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return Result.Failure<CreateRedirectResponse>(languageCodeResult.Error);
        }

        if (!Enum.TryParse<RedirectTargetKind>(request.TargetKind, ignoreCase: true, out var targetKind))
        {
            return Result.Failure<CreateRedirectResponse>(Error.Validation(
                "Redirect.InvalidTargetKind", $"'{request.TargetKind}' is not a recognized target kind."));
        }

        if (!Enum.TryParse<RedirectStatusCode>(request.StatusCode, ignoreCase: true, out var statusCode))
        {
            return Result.Failure<CreateRedirectResponse>(Error.Validation(
                "Redirect.InvalidStatusCode", $"'{request.StatusCode}' is not a recognized status code."));
        }

        if (targetKind == RedirectTargetKind.ContentItem)
        {
            if (request.TargetContentItemId is null
                || await contentItemRepository.GetByIdAsync(request.TargetContentItemId.Value, cancellationToken) is null)
            {
                return Result.Failure<CreateRedirectResponse>(Error.NotFound(
                    "Redirect.TargetContentItemNotFound", $"Content item '{request.TargetContentItemId}' could not be found."));
            }
        }

        var redirectResult = Redirect.Create(
            languageCodeResult.Value, request.FromPath, targetKind, request.TargetContentItemId, request.TargetPath, statusCode,
            currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (redirectResult.IsFailure)
        {
            return Result.Failure<CreateRedirectResponse>(redirectResult.Error);
        }

        var redirect = redirectResult.Value;

        var collisionCheck = await RedirectCollisionGuard.CheckAsync(redirect, contentItemRepository, redirectRepository, cancellationToken);
        if (collisionCheck.IsFailure)
        {
            return Result.Failure<CreateRedirectResponse>(collisionCheck.Error);
        }

        redirectRepository.Add(redirect);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateRedirectResponse(redirect.Id, redirect.FromPath));
    }
}
