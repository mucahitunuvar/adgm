using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;

public sealed record UploadMediaAssetCommand(
    Stream Content, string FileName, string ContentType, string? Folder, string? AltText, string? Caption)
    : IRequest<Result<UploadMediaAssetResponse>>;
