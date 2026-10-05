using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePopupTranslation;

public sealed class UpdatePopupTranslationCommandHandler(
    IPopupRepository popupRepository,
    IHtmlContentSanitizer htmlContentSanitizer,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePopupTranslationCommand, Result>
{
    public async Task<Result> Handle(UpdatePopupTranslationCommand request, CancellationToken cancellationToken)
    {
        var popup = await popupRepository.GetByIdAsync(request.Id, cancellationToken);
        if (popup is null)
        {
            return Result.Failure(Error.NotFound("Popup.NotFound", $"Popup '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(popup.RowVersion))
        {
            return Result.Failure(Error.Conflict("Popup.ConcurrencyConflict", "The popup was changed by someone else. Reload and try again."));
        }

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        // §1 "Zengin metin alanları handler'da IHtmlContentSanitizer ile temizlenir" - only for Modal;
        // a Banner's body must arrive already as plain text (PopupTranslation rejects any HTML in it).
        var body = popup.DisplayMode == PopupDisplayMode.Modal ? htmlContentSanitizer.Sanitize(request.Body ?? string.Empty) : request.Body;

        var setResult = popup.SetTranslation(
            languageCodeResult.Value, request.Title, body, request.ButtonLabel, currentUserContext.UserId!.Value,
            timeProvider.GetUtcNow().UtcDateTime);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
