using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;

public sealed class PublishLegalDocumentDraftCommandHandler(
    ILegalDocumentRepository legalDocumentRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<PublishLegalDocumentDraftCommand, Result>
{
    public async Task<Result> Handle(PublishLegalDocumentDraftCommand request, CancellationToken cancellationToken)
    {
        var document = await legalDocumentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (document is null)
        {
            return Result.Failure(Error.NotFound("LegalDocument.NotFound", $"Legal document '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(document.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "LegalDocument.ConcurrencyConflict", "The legal document was changed by someone else. Reload and try again."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure(Error.Failure("LegalDocument.NoDefaultLanguage", "No default site language is configured."));
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var publishResult = document.PublishDraft(defaultLanguage.Code, request.EffectiveAtUtc, currentUserContext.UserId!.Value, now);
        if (publishResult.IsFailure)
        {
            return publishResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
