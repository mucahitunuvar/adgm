using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsContact;

public sealed class UpdateSiteSettingsContactCommandHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSiteSettingsContactCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteSettingsContactCommand request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken);
        var isNew = settings is null;
        settings ??= SiteSettings.CreateDefault();

        if (!isNew && !request.RowVersion.SequenceEqual(settings.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "SiteSettings.ConcurrencyConflict", "Site settings were changed by someone else. Reload and try again."));
        }

        var contactResult = ContactInfo.Create(request.Address, request.Phone, request.Email, request.WhatsApp, request.MapEmbedUrl);
        if (contactResult.IsFailure)
        {
            return contactResult;
        }

        var socialLinks = request.SocialLinks
            .Select(l => SocialLink.Create(l.Platform, l.Url, l.SortOrder))
            .ToList();

        if (isNew)
        {
            siteSettingsRepository.Add(settings);
        }

        var userId = currentUserContext.UserId!.Value;
        var now = DateTime.UtcNow;

        settings.UpdateContact(contactResult.Value, userId, now);
        settings.ReplaceSocialLinks(socialLinks, userId, now);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
