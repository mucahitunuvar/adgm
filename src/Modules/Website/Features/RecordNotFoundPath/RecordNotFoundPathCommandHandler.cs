using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Features.RecordNotFoundPath;

public sealed class RecordNotFoundPathCommandHandler(
    INotFoundLogRepository notFoundLogRepository,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<RecordNotFoundPathCommandHandler> logger)
    : IRequestHandler<RecordNotFoundPathCommand, Result>
{
    // ADR-024 §15: the hard cap on distinct paths tracked - once reached, an already-known path still
    // has its counter bumped, but no new path is recorded.
    private const int MaxDistinctPaths = 10_000;

    public async Task<Result> Handle(RecordNotFoundPathCommand request, CancellationToken cancellationToken)
    {
        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var languageCode = languageCodeResult.Value;
        var normalizedPath = (request.Path ?? string.Empty).Trim();
        var now = timeProvider.GetUtcNow().UtcDateTime;

        if (await TryBumpExistingHitAsync(languageCode, normalizedPath, now, cancellationToken))
        {
            return Result.Success();
        }

        if (await notFoundLogRepository.CountAsync(cancellationToken) >= MaxDistinctPaths)
        {
            return Result.Success();
        }

        var createResult = NotFoundLog.Create(languageCode, request.Path, now);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        var notFoundLog = createResult.Value;
        notFoundLogRepository.Add(notFoundLog);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // (LanguageCode, Path) is unique - a concurrent request most likely logged the same path
            // first between our TryBumpExistingHitAsync check above and this insert. The failed insert
            // must be detached before any further SaveChangesAsync on this same UnitOfWork, or it would
            // be retried (and fail again) alongside the bump below.
            notFoundLogRepository.DetachFailedAdd(notFoundLog);

            try
            {
                if (await TryBumpExistingHitAsync(languageCode, normalizedPath, now, cancellationToken))
                {
                    return Result.Success();
                }
            }
            catch (Exception retryEx) when (retryEx is not OperationCanceledException)
            {
                logger.LogWarning(
                    retryEx, "Retry after failing to record not-found path '{Path}' for language '{LanguageCode}' also failed.",
                    normalizedPath, languageCode.Value);
                return Result.Success();
            }

            // A recording failure must never surface to the visitor (ADR-024 §15) - the resolution
            // response is unaffected either way, only this diagnostic hit is lost.
            logger.LogWarning(
                ex, "Failed to record not-found path '{Path}' for language '{LanguageCode}'.", normalizedPath, languageCode.Value);
            return Result.Success();
        }
    }

    private async Task<bool> TryBumpExistingHitAsync(
        LanguageCode languageCode, string normalizedPath, DateTime now, CancellationToken cancellationToken)
    {
        var existing = await notFoundLogRepository.GetByPathAsync(languageCode, normalizedPath, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.RecordHit(now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
