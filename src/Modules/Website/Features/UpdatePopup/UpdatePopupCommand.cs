using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.Popups;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePopup;

public sealed record UpdatePopupCommand(
    Guid Id,
    byte[] RowVersion,
    string DisplayMode,
    Guid? ImageMediaId,
    LinkTargetDto? Link,
    PopupTargetingInput Targeting,
    string DeviceTarget,
    DateTime? PublishAtUtc,
    DateTime? UnpublishAtUtc,
    int DelaySeconds,
    string Frequency,
    int? FrequencyDays,
    bool Dismissible,
    int Priority) : IRequest<Result>;
