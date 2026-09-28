using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentTypeTranslation;

public sealed class UpdateContentTypeTranslationCommandHandler(
    IContentTypeRepository contentTypeRepository,
    IContentItemRepository contentItemRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ContentPathCascadeService contentPathCascadeService,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateContentTypeTranslationCommand, Result>
{
    // ADR-024 §4.3 Görev 4: a RoutePrefix change recomputes every root item of this type (in this
    // language) plus their whole descendant subtree, synchronously, in one transaction - this project's
    // content volume is low enough for that to be safe, but not unbounded. Root items alone exceeding
    // this is already an extreme case for this project, so it is checked up front rather than walking
    // the full descendant tree twice (once to count, once to cascade).
    private const int MaxAffectedRootItems = 1000;

    public async Task<Result> Handle(UpdateContentTypeTranslationCommand request, CancellationToken cancellationToken)
    {
        var contentType = await contentTypeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure(Error.NotFound("ContentType.NotFound", $"Content type '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(contentType.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "ContentType.ConcurrencyConflict", "The content type was changed by someone else. Reload and try again."));
        }

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var languageCode = languageCodeResult.Value;

        var routePrefixCheck = await RoutePrefixGuard.CheckAsync(
            request.RoutePrefix, languageCode, contentType.Id, siteLanguageRepository, contentTypeRepository, cancellationToken);
        if (routePrefixCheck.IsFailure)
        {
            return routePrefixCheck;
        }

        var seoResult = SeoMetadata.Create(
            request.Seo.MetaTitle, request.Seo.MetaDescription, request.Seo.MetaKeywords, request.Seo.OgTitle,
            request.Seo.OgDescription, request.Seo.OgImageMediaId, request.Seo.CanonicalUrl, request.Seo.NoIndex);
        if (seoResult.IsFailure)
        {
            return seoResult;
        }

        var previousRoutePrefix = contentType.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)?.RoutePrefix;
        var userId = currentUserContext.UserId!.Value;
        var now = DateTime.UtcNow;

        var rootItems = new List<ContentItem>();
        if (previousRoutePrefix is not null && previousRoutePrefix != request.RoutePrefix)
        {
            var allRootItems = await contentItemRepository.GetRootItemsByContentTypeIdAsync(contentType.Id, cancellationToken);
            if (allRootItems.Count > MaxAffectedRootItems)
            {
                return Result.Failure(Error.Conflict(
                    "ContentType.TooManyAffectedContentItems",
                    $"Changing this route prefix would affect more than {MaxAffectedRootItems} content items; this is not supported in one operation."));
            }

            rootItems = allRootItems.Where(item => item.Translations.Any(t => t.LanguageCode == languageCode)).ToList();
        }

        var setResult = contentType.SetTranslation(languageCode, request.Name, request.RoutePrefix, seoResult.Value, userId, now);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        var newRoutePrefix = contentType.Translations.First(t => t.LanguageCode == languageCode).RoutePrefix;

        foreach (var rootItem in rootItems)
        {
            var oldFullPath = rootItem.Translations.First(t => t.LanguageCode == languageCode).FullPath;
            rootItem.RecomputeFullPath(languageCode, newRoutePrefix, [], userId, now);
            var newFullPath = rootItem.Translations.First(t => t.LanguageCode == languageCode).FullPath;

            if (oldFullPath == newFullPath)
            {
                continue;
            }

            var redirectResult = await contentPathCascadeService.CreateAutomaticRedirectAsync(
                languageCode, oldFullPath, newFullPath, rootItem.Id, userId, now, cancellationToken);
            if (redirectResult.IsFailure)
            {
                return redirectResult;
            }

            var cascadeResult = await contentPathCascadeService.CascadeDescendantPathsAsync(
                rootItem, languageCode, newRoutePrefix, [], userId, now, cancellationToken);
            if (cascadeResult.IsFailure)
            {
                return cascadeResult;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
