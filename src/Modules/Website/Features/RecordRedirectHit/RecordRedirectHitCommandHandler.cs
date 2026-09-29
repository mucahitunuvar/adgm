using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Features.RecordRedirectHit;

public sealed class RecordRedirectHitCommandHandler(
    IRedirectRepository redirectRepository,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<RecordRedirectHitCommandHandler> logger)
    : IRequestHandler<RecordRedirectHitCommand, Result>
{
    public async Task<Result> Handle(RecordRedirectHitCommand request, CancellationToken cancellationToken)
    {
        var redirect = await redirectRepository.GetByIdAsync(request.RedirectId, cancellationToken);
        if (redirect is null)
        {
            // Best-effort (ADR-024 §15, post-Faz-1a fix): the redirect could have been edited or
            // deleted between resolution and this follow-up command - nothing to bump, not a failure.
            return Result.Success();
        }

        redirect.RecordHit(timeProvider.GetUtcNow().UtcDateTime);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The hit counter is not required to be exact (ADR-024 §15) - a concurrency conflict (e.g.
            // the redirect was deleted concurrently) is silently given up on rather than retried.
            logger.LogWarning(ex, "Failed to record a hit for redirect '{RedirectId}'.", request.RedirectId);
        }

        return Result.Success();
    }
}
