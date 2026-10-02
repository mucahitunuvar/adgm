using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetHomeLayoutPreview;

// Faz 2 Görev 5 master prompt §5.1: never cached - an editor must see the effect of an unpublished
// draft change immediately, not up to the public response's TTL later.
public sealed class GetHomeLayoutPreviewQueryHandler(
    ISiteLanguageRepository siteLanguageRepository, IPageLayoutRepository pageLayoutRepository, PublicPageLayoutResolver publicPageLayoutResolver,
    TimeProvider timeProvider)
    : IRequestHandler<GetHomeLayoutPreviewQuery, Result<GetHomeLayoutPreviewResponse>>
{
    public async Task<Result<GetHomeLayoutPreviewResponse>> Handle(GetHomeLayoutPreviewQuery request, CancellationToken cancellationToken)
    {
        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var defaultLanguage = activeLanguages.First(l => l.IsDefault);
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? defaultLanguage;

        var layout = await pageLayoutRepository.GetHomeAsync(cancellationToken)
            ?? throw new InvalidOperationException("The home page layout is missing its seeded row.");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var blocks = await publicPageLayoutResolver.ResolveAsync(
            layout.DraftBlocks, resolvedLanguage.Code, defaultLanguage.Code, now, cancellationToken);

        return Result.Success(new GetHomeLayoutPreviewResponse(blocks));
    }
}
