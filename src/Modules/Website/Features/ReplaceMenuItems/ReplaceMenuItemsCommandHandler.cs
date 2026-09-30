using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceMenuItems;

// Faz 2 Görev 1 master prompt §1.2: replaces a Menu's entire item tree in one call (the editor's
// drag-and-drop UI always saves the whole tree, never a single node). Every item gets a freshly
// generated real id on every save, resolved from the request's client-supplied TempId - the same
// "whole collection replaced, ids regenerated" shape SetContentItemGallery already uses for a flat
// child collection; nothing outside this Menu ever references a MenuItem's id, so regenerating it on
// every save is harmless.
public sealed class ReplaceMenuItemsCommandHandler(
    IMenuRepository menuRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ReplaceMenuItemsCommand, Result>
{
    public async Task<Result> Handle(ReplaceMenuItemsCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<MenuLocation>(request.Location, ignoreCase: true, out var location))
        {
            return Result.Failure(Error.Validation("Menu.LocationInvalid", "Location must be one of 'Header', 'Utility' or 'Footer'."));
        }

        var menu = await menuRepository.GetByLocationAsync(location, cancellationToken);
        if (menu is null)
        {
            return Result.Failure(Error.NotFound("Menu.NotFound", $"Menu for location '{location}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(menu.RowVersion))
        {
            return Result.Failure(Error.Conflict("Menu.ConcurrencyConflict", "The menu was changed by someone else. Reload and try again."));
        }

        var duplicateTempId = request.Items.GroupBy(i => i.TempId, StringComparer.Ordinal).FirstOrDefault(g => g.Count() > 1);
        if (duplicateTempId is not null)
        {
            return Result.Failure(Error.Validation("Menu.DuplicateTempId", $"Temporary id '{duplicateTempId.Key}' appears more than once."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No default site language is configured.");

        var idByTempId = request.Items.ToDictionary(i => i.TempId, _ => Guid.NewGuid(), StringComparer.Ordinal);

        var items = new List<MenuItem>();
        foreach (var input in request.Items)
        {
            var buildResult = BuildItem(input, idByTempId, defaultLanguage.Code);
            if (buildResult.IsFailure)
            {
                return Result.Failure(buildResult.Error);
            }

            items.Add(buildResult.Value);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var replaceResult = menu.ReplaceItems(items, currentUserContext.UserId!.Value, now);
        if (replaceResult.IsFailure)
        {
            return replaceResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Menu only feeds the public-site bootstrap response (GetPublicSite), never the public-content
        // list/detail/route-resolution cache - InvalidatePublicContent would be a no-op guess here.
        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }

    private static Result<MenuItem> BuildItem(
        MenuItemTreeInput input, IReadOnlyDictionary<string, Guid> idByTempId, LanguageCode defaultLanguageCode)
    {
        Guid? parentId = null;
        if (input.ParentTempId is not null)
        {
            if (!idByTempId.TryGetValue(input.ParentTempId, out var resolvedParentId))
            {
                return Result.Failure<MenuItem>(Error.Validation(
                    "Menu.ParentTempIdNotFound",
                    $"Menu item '{input.TempId}' references an unknown parent temporary id '{input.ParentTempId}'."));
            }

            parentId = resolvedParentId;
        }

        var linkTargetResult = BuildLinkTarget(input.Link);
        if (linkTargetResult.IsFailure)
        {
            return Result.Failure<MenuItem>(linkTargetResult.Error);
        }

        var translations = new List<MenuItemTranslation>();
        foreach (var translationInput in input.Translations)
        {
            var languageCodeResult = LanguageCode.Create(translationInput.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure<MenuItem>(languageCodeResult.Error);
            }

            var translationResult = MenuItemTranslation.Create(languageCodeResult.Value, translationInput.Label);
            if (translationResult.IsFailure)
            {
                return Result.Failure<MenuItem>(translationResult.Error);
            }

            translations.Add(translationResult.Value);
        }

        // Faz 2 master prompt §1 "Çeviri kuralı": the default language's translation is required.
        if (translations.All(t => t.LanguageCode != defaultLanguageCode))
        {
            return Result.Failure<MenuItem>(Error.Validation(
                "MenuItem.DefaultLanguageTranslationRequired",
                $"Menu item '{input.TempId}' must have a translation in the default language '{defaultLanguageCode}'."));
        }

        return MenuItem.Create(
            idByTempId[input.TempId], parentId, input.SortOrder, input.IsActive, linkTargetResult.Value, input.OpenInNewTab, input.IconKey,
            translations);
    }

    private static Result<LinkTarget> BuildLinkTarget(MenuItemLinkInput? link)
    {
        if (link is null)
        {
            return LinkTarget.CreateEmpty();
        }

        if (!Enum.TryParse<LinkTargetKind>(link.Kind, ignoreCase: true, out var kind))
        {
            return Result.Failure<LinkTarget>(Error.Validation("MenuItem.LinkKindInvalid", $"Unknown link kind '{link.Kind}'."));
        }

        return kind switch
        {
            LinkTargetKind.None => LinkTarget.CreateEmpty(),
            LinkTargetKind.Content => LinkTarget.ForContent(link.ContentItemId ?? Guid.Empty),
            LinkTargetKind.ContentTypeListing => LinkTarget.ForContentTypeListing(link.ContentTypeId ?? Guid.Empty),
            LinkTargetKind.InternalPath => LinkTarget.ForInternalPath(link.InternalPath),
            LinkTargetKind.ExternalUrl => LinkTarget.ForExternalUrl(link.ExternalUrl),
            _ => Result.Failure<LinkTarget>(Error.Validation("MenuItem.LinkKindInvalid", $"Unknown link kind '{link.Kind}'.")),
        };
    }
}
