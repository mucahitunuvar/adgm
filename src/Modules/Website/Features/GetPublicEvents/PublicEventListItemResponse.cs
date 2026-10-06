namespace GenclikMerkezi.Modules.Website.Features.GetPublicEvents;

public sealed record PublicEventListItemResponse(
    Guid Id, string Title, string Path, PublicEventImageResponse? CoverImage, PublicEventSummaryResponse Event);
