using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Parsing;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateRedirect;

public sealed class UpdateRedirectCommandHandler(
    IRedirectRepository redirectRepository,
    IContentItemRepository contentItemRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateRedirectCommand, Result>
{
    public async Task<Result> Handle(UpdateRedirectCommand request, CancellationToken cancellationToken)
    {
        var redirect = await redirectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (redirect is null)
        {
            return Result.Failure(Error.NotFound("Redirect.NotFound", $"Redirect '{request.Id}' could not be found."));
        }

        if (!EnumParsing.TryParseDefined<RedirectTargetKind>(request.TargetKind, out var targetKind))
        {
            return Result.Failure(Error.Validation("Redirect.InvalidTargetKind", $"'{request.TargetKind}' is not a recognized target kind."));
        }

        if (!EnumParsing.TryParseDefined<RedirectStatusCode>(request.StatusCode, out var statusCode))
        {
            return Result.Failure(Error.Validation("Redirect.InvalidStatusCode", $"'{request.StatusCode}' is not a recognized status code."));
        }

        if (targetKind == RedirectTargetKind.ContentItem)
        {
            if (request.TargetContentItemId is null
                || await contentItemRepository.GetByIdAsync(request.TargetContentItemId.Value, cancellationToken) is null)
            {
                return Result.Failure(Error.NotFound(
                    "Redirect.TargetContentItemNotFound", $"Content item '{request.TargetContentItemId}' could not be found."));
            }
        }

        // Reuses Redirect's own NormalizeTargetPath rather than re-deriving a candidate with a laxer
        // Trim('/') (that duplicate used to let a backslash/".."/control-character target slip past
        // this chain check unnoticed - see Redirect.cs). A normalization failure here is not surfaced
        // directly: it is left for redirect.Update(...) below to report as the real validation error.
        if (targetKind == RedirectTargetKind.Path && !string.IsNullOrWhiteSpace(request.TargetPath))
        {
            var normalizedTargetResult = Redirect.NormalizeTargetPath(request.TargetPath);
            if (normalizedTargetResult.IsSuccess
                && await redirectRepository.GetByFromPathAsync(redirect.LanguageCode, normalizedTargetResult.Value, cancellationToken) is { } chained
                && chained.Id != redirect.Id)
            {
                return Result.Failure(Error.Conflict(
                    "Redirect.TargetWouldCreateChain",
                    $"'{normalizedTargetResult.Value}' is itself the source of another redirect; chained redirects are not supported."));
            }
        }

        var updateResult = redirect.Update(
            targetKind, request.TargetContentItemId, request.TargetPath, statusCode, currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicContent(cacheService);

        return Result.Success();
    }
}
