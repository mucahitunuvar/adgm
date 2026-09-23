using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateMediaAsset;

public sealed record UpdateMediaAssetCommand(
    Guid Id,
    string? Folder,
    string? Source,
    string? UsagePermissionNote,
    bool ContainsPersonalData,
    IReadOnlyList<UpdateMediaAssetTranslationInput>? Translations)
    : IRequest<Result>;
