using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.RecordNotFoundPath;

public sealed class RecordNotFoundPathCommandHandler(
    INotFoundLogRepository notFoundLogRepository, [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
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

        var normalizedPath = (request.Path ?? string.Empty).Trim();
        var now = DateTime.UtcNow;

        var existing = await notFoundLogRepository.GetByPathAsync(languageCodeResult.Value, normalizedPath, cancellationToken);
        if (existing is not null)
        {
            existing.RecordHit(now);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        if (await notFoundLogRepository.CountAsync(cancellationToken) >= MaxDistinctPaths)
        {
            return Result.Success();
        }

        var createResult = NotFoundLog.Create(languageCodeResult.Value, request.Path, now);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        notFoundLogRepository.Add(createResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
