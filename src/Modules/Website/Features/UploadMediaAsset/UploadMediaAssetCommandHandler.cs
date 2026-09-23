using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;

public sealed class UploadMediaAssetCommandHandler(
    IMediaAssetRepository mediaAssetRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IImageProcessor imageProcessor,
    IFileStorageService fileStorageService,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UploadMediaAssetCommand, Result<UploadMediaAssetResponse>>
{
    private static readonly string[] ImageExtensions = ["jpg", "jpeg", "png", "webp"];
    private static readonly string[] DocumentExtensions = ["pdf", "docx", "xlsx"];

    public async Task<Result<UploadMediaAssetResponse>> Handle(UploadMediaAssetCommand request, CancellationToken cancellationToken)
    {
        var folderResult = MediaFolder.Create(request.Folder);
        if (folderResult.IsFailure)
        {
            return Result.Failure<UploadMediaAssetResponse>(folderResult.Error);
        }

        var extension = Path.GetExtension(request.FileName).TrimStart('.').ToLowerInvariant();
        var kind = ImageExtensions.Contains(extension)
            ? MediaAssetKind.Image
            : DocumentExtensions.Contains(extension) ? MediaAssetKind.Document : (MediaAssetKind?)null;

        if (kind is null)
        {
            return Result.Failure<UploadMediaAssetResponse>(Error.Validation(
                "MediaAsset.UnsupportedFileType",
                $"File extension '.{extension}' is not supported. Allowed: {string.Join(", ", ImageExtensions.Concat(DocumentExtensions))}."));
        }

        await using var buffer = new MemoryStream();
        await request.Content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        var mediaAssetId = Guid.NewGuid();
        var userId = currentUserContext.UserId!.Value;
        var now = DateTime.UtcNow;

        Result<MediaAsset> mediaAssetResult;

        if (kind == MediaAssetKind.Image)
        {
            mediaAssetResult = await CreateImageAssetAsync(mediaAssetId, request, bytes, folderResult.Value, userId, now, cancellationToken);
        }
        else
        {
            mediaAssetResult = await CreateDocumentAssetAsync(mediaAssetId, request, bytes, folderResult.Value, userId, now, cancellationToken);
        }

        if (mediaAssetResult.IsFailure)
        {
            return Result.Failure<UploadMediaAssetResponse>(mediaAssetResult.Error);
        }

        var mediaAsset = mediaAssetResult.Value;

        if (!string.IsNullOrWhiteSpace(request.AltText) || !string.IsNullOrWhiteSpace(request.Caption))
        {
            var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
            if (defaultLanguage is not null)
            {
                mediaAsset.SetTranslation(defaultLanguage.Code, request.AltText, request.Caption, userId, now);
            }
        }

        mediaAssetRepository.Add(mediaAsset);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(await BuildResponseAsync(mediaAsset, cancellationToken));
    }

    private async Task<Result<MediaAsset>> CreateImageAssetAsync(
        Guid mediaAssetId, UploadMediaAssetCommand request, byte[] bytes, MediaFolder folder,
        Guid userId, DateTime now, CancellationToken cancellationToken)
    {
        var policyResult = MediaValidationPolicies.Image.Validate(request.FileName, request.ContentType, bytes.Length);
        if (policyResult.IsFailure)
        {
            return Result.Failure<MediaAsset>(policyResult.Error);
        }

        var processedResult = imageProcessor.Process(bytes);
        if (processedResult.IsFailure)
        {
            return Result.Failure<MediaAsset>(processedResult.Error);
        }

        var processed = processedResult.Value;

        var originalUploadResult = await fileStorageService.UploadAsync(
            new MemoryStream(processed.OriginalBytes), request.FileName, request.ContentType,
            FileCategory.WebsiteImage, "MediaAsset", mediaAssetId, MediaValidationPolicies.ProcessedImageOutput, cancellationToken);
        if (originalUploadResult.IsFailure)
        {
            return Result.Failure<MediaAsset>(originalUploadResult.Error);
        }

        var variants = new List<MediaAssetVariant>();
        var fileNameStem = Path.GetFileNameWithoutExtension(request.FileName);

        foreach (var processedVariant in processed.Variants)
        {
            var variantUploadResult = await fileStorageService.UploadAsync(
                new MemoryStream(processedVariant.Bytes), $"{fileNameStem}-{processedVariant.VariantName}.webp", "image/webp",
                FileCategory.WebsiteImage, "MediaAsset", mediaAssetId, MediaValidationPolicies.ProcessedImageOutput, cancellationToken);
            if (variantUploadResult.IsFailure)
            {
                return Result.Failure<MediaAsset>(variantUploadResult.Error);
            }

            variants.Add(MediaAssetVariant.Create(
                processedVariant.VariantName, variantUploadResult.Value, processedVariant.Width, processedVariant.Height));
        }

        return MediaAsset.Create(
            mediaAssetId, MediaAssetKind.Image, originalUploadResult.Value, variants,
            processed.Width, processed.Height, folder, source: null, usagePermissionNote: null,
            containsPersonalData: false, userId, now);
    }

    private async Task<Result<MediaAsset>> CreateDocumentAssetAsync(
        Guid mediaAssetId, UploadMediaAssetCommand request, byte[] bytes, MediaFolder folder,
        Guid userId, DateTime now, CancellationToken cancellationToken)
    {
        var policyResult = MediaValidationPolicies.Document.Validate(request.FileName, request.ContentType, bytes.Length);
        if (policyResult.IsFailure)
        {
            return Result.Failure<MediaAsset>(policyResult.Error);
        }

        var extension = Path.GetExtension(request.FileName).TrimStart('.').ToLowerInvariant();
        var hasValidSignature = extension == "pdf"
            ? FileSignatureValidator.HasPdfSignature(bytes)
            : FileSignatureValidator.HasZipSignature(bytes);

        if (!hasValidSignature)
        {
            return Result.Failure<MediaAsset>(Error.Validation(
                "MediaAsset.InvalidFileSignature", "The file's content does not match its declared type."));
        }

        var uploadResult = await fileStorageService.UploadAsync(
            new MemoryStream(bytes), request.FileName, request.ContentType,
            FileCategory.WebsiteDocument, "MediaAsset", mediaAssetId, MediaValidationPolicies.Document, cancellationToken);
        if (uploadResult.IsFailure)
        {
            return Result.Failure<MediaAsset>(uploadResult.Error);
        }

        return MediaAsset.Create(
            mediaAssetId, MediaAssetKind.Document, uploadResult.Value, [],
            width: null, height: null, folder, source: null, usagePermissionNote: null,
            containsPersonalData: false, userId, now);
    }

    private async Task<UploadMediaAssetResponse> BuildResponseAsync(MediaAsset mediaAsset, CancellationToken cancellationToken)
    {
        var originalUrl = await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);

        var variants = new List<UploadMediaAssetVariantResponse>();
        foreach (var variant in mediaAsset.Variants)
        {
            var url = await fileStorageService.GetUrlAsync(variant.File.FileKey, cancellationToken);
            variants.Add(new UploadMediaAssetVariantResponse(variant.VariantName, url, variant.Width, variant.Height));
        }

        return new UploadMediaAssetResponse(
            mediaAsset.Id, mediaAsset.Kind.ToString(), originalUrl, mediaAsset.Width, mediaAsset.Height,
            mediaAsset.Folder.Value, variants);
    }
}
