using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.LinkTargets;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetMenuByLocation;

public sealed class GetMenuByLocationQueryHandler(
    IMenuRepository menuRepository,
    ISiteLanguageRepository siteLanguageRepository,
    LinkTargetResolver linkTargetResolver,
    TimeProvider timeProvider)
    : IRequestHandler<GetMenuByLocationQuery, Result<MenuResponse>>
{
    public async Task<Result<MenuResponse>> Handle(GetMenuByLocationQuery request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<MenuLocation>(request.Location, ignoreCase: true, out var location))
        {
            return Result.Failure<MenuResponse>(Error.Validation(
                "Menu.LocationInvalid", "Location must be one of 'Header', 'Utility' or 'Footer'."));
        }

        var menu = await menuRepository.GetByLocationAsync(location, cancellationToken);
        if (menu is null)
        {
            return Result.Failure<MenuResponse>(Error.NotFound("Menu.NotFound", $"Menu for location '{location}' could not be found."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No default site language is configured.");

        var targets = menu.Items.Select(i => i.LinkTarget).Where(t => !t.IsEmpty).ToList();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var resolutions = await linkTargetResolver.ResolveManyAsync(targets, defaultLanguage.Code, defaultLanguage.Code, now, cancellationToken);

        return Result.Success(Map(menu, resolutions));
    }

    // ADR-024 §1.2: TargetStatus/BrokenLinkReason is resolved against the site's default language
    // only - the admin editor's own working language is out of this Görev's scope.
    internal static MenuResponse Map(Menu menu, IReadOnlyDictionary<LinkTarget, LinkTargetResolution> resolutions)
    {
        var items = menu.Items.Select(item =>
        {
            MenuItemLinkResponse? link = item.LinkTarget.IsEmpty
                ? null
                : new MenuItemLinkResponse(
                    item.LinkTarget.Kind.ToString(), item.LinkTarget.ContentItemId, item.LinkTarget.ContentTypeId,
                    item.LinkTarget.InternalPath, item.LinkTarget.ExternalUrl);

            string? brokenLinkReason = null;
            if (!item.LinkTarget.IsEmpty && resolutions.TryGetValue(item.LinkTarget, out var resolution) && !resolution.IsResolved)
            {
                brokenLinkReason = resolution.UnresolvedReason.ToString();
            }

            var translations = item.Translations
                .Select(t => new MenuItemTranslationResponse(t.LanguageCode.Value, t.Label))
                .ToList();

            return new MenuItemResponse(
                item.Id, item.ParentId, item.SortOrder, item.IsActive, link, item.OpenInNewTab, item.IconKey, translations,
                brokenLinkReason);
        }).ToList();

        return new MenuResponse(menu.Id, menu.Location.ToString(), menu.RowVersion, items);
    }
}
