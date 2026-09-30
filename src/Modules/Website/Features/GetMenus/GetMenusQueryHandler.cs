using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.LinkTargets;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.GetMenuByLocation;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetMenus;

public sealed class GetMenusQueryHandler(
    IMenuRepository menuRepository,
    ISiteLanguageRepository siteLanguageRepository,
    LinkTargetResolver linkTargetResolver,
    TimeProvider timeProvider)
    : IRequestHandler<GetMenusQuery, Result<IReadOnlyList<MenuResponse>>>
{
    public async Task<Result<IReadOnlyList<MenuResponse>>> Handle(GetMenusQuery request, CancellationToken cancellationToken)
    {
        var menus = await menuRepository.GetAllAsync(cancellationToken);

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No default site language is configured.");

        // Every item of every menu resolved in one batched call (Görev 1's own "N+1 yok" rule applies
        // just as much across menus as within one).
        var allTargets = menus.SelectMany(m => m.Items).Select(i => i.LinkTarget).Where(t => !t.IsEmpty).ToList();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var resolutions = await linkTargetResolver.ResolveManyAsync(allTargets, defaultLanguage.Code, defaultLanguage.Code, now, cancellationToken);

        IReadOnlyList<MenuResponse> responses = menus.Select(menu => GetMenuByLocationQueryHandler.Map(menu, resolutions)).ToList();

        return Result.Success(responses);
    }
}
