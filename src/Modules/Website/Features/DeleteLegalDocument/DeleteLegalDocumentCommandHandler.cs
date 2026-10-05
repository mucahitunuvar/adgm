using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteLegalDocument;

// §12.1 "Hiç yayınlanmış sürümü olmayan doküman silinebilir; bir form tarafından kullanılıyorsa
// silinemez": the second check uses ILegalDocumentUsageChecker's always-empty Görev 2 stub - once
// Görev 3 adds FormDefinition, this handler needs no changes, only the registered implementation does
// (mirrors DeleteSliderCommandHandler's own ISliderUsageChecker usage).
public sealed class DeleteLegalDocumentCommandHandler(
    ILegalDocumentRepository legalDocumentRepository,
    ILegalDocumentUsageChecker legalDocumentUsageChecker,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteLegalDocumentCommand, Result>
{
    public async Task<Result> Handle(DeleteLegalDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await legalDocumentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (document is null)
        {
            return Result.Failure(Error.NotFound("LegalDocument.NotFound", $"Legal document '{request.Id}' could not be found."));
        }

        if (document.HasEverBeenPublished)
        {
            return Result.Failure(Error.Conflict(
                "LegalDocument.HasPublishedVersion", "A legal document that has ever had a published version cannot be deleted."));
        }

        var usages = await legalDocumentUsageChecker.GetUsagesAsync(document.Id, cancellationToken);
        if (usages.Count > 0)
        {
            var usageDescriptions = string.Join(", ", usages.Select(u => u.Description));
            return Result.Failure(Error.Conflict(
                "LegalDocument.InUse", $"This legal document is in use and cannot be deleted: {usageDescriptions}."));
        }

        legalDocumentRepository.Remove(document);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
