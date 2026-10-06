using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateThirdPartyScript;

public sealed record UpdateThirdPartyScriptCommand(
    Guid Id,
    byte[] RowVersion,
    string? Provider,
    string? MeasurementId,
    string? ContainerId,
    string? PixelId,
    string? Src,
    bool Async,
    bool Defer,
    string? Category,
    string? Placement,
    int SortOrder) : IRequest<Result>;
