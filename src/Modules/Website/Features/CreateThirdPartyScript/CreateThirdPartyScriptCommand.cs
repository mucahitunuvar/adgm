using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateThirdPartyScript;

public sealed record CreateThirdPartyScriptCommand(
    string? Provider,
    string? MeasurementId,
    string? ContainerId,
    string? PixelId,
    string? Src,
    bool Async,
    bool Defer,
    string? Category,
    string? Placement,
    int SortOrder,
    string? DefaultLanguageName,
    string? DefaultLanguagePurpose) : IRequest<Result<CreateThirdPartyScriptResponse>>;
