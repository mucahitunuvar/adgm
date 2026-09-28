using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentTypeTranslation;

public sealed class UpdateContentTypeTranslationCommandHandler(
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateContentTypeTranslationCommand, Result>
{
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

        var routePrefixCheck = await RoutePrefixGuard.CheckAsync(
            request.RoutePrefix, languageCodeResult.Value, contentType.Id, siteLanguageRepository, contentTypeRepository, cancellationToken);
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

        var setResult = contentType.SetTranslation(
            languageCodeResult.Value, request.Name, request.RoutePrefix, seoResult.Value, currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
