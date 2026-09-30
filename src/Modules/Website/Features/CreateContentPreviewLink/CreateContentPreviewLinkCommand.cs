using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentPreviewLink;

public sealed record CreateContentPreviewLinkCommand(Guid ContentItemId, string? LanguageCode, int? DurationHours)
    : IRequest<Result<CreateContentPreviewLinkResponse>>;
