using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateMediaAsset;

public sealed class UpdateMediaAssetCommandHandler(
    IMediaAssetRepository mediaAssetRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMediaAssetCommand, Result>
{
    public async Task<Result> Handle(UpdateMediaAssetCommand request, CancellationToken cancellationToken)
    {
        var mediaAsset = await mediaAssetRepository.GetByIdAsync(request.Id, cancellationToken);
        if (mediaAsset is null)
        {
            return Result.Failure(Error.NotFound("MediaAsset.NotFound", "The specified media asset could not be found."));
        }

        var folderResult = MediaFolder.Create(request.Folder);
        if (folderResult.IsFailure)
        {
            return folderResult;
        }

        var resolvedTranslations = new List<(LanguageCode LanguageCode, string? AltText, string? Caption)>();
        foreach (var translation in request.Translations ?? [])
        {
            var languageCodeResult = LanguageCode.Create(translation.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return languageCodeResult;
            }

            resolvedTranslations.Add((languageCodeResult.Value, translation.AltText, translation.Caption));
        }

        var userId = currentUserContext.UserId!.Value;
        var now = DateTime.UtcNow;

        var updateResult = mediaAsset.UpdateMetadata(
            folderResult.Value, request.Source, request.UsagePermissionNote, request.ContainsPersonalData, userId, now);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        foreach (var (languageCode, altText, caption) in resolvedTranslations)
        {
            mediaAsset.SetTranslation(languageCode, altText, caption, userId, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
