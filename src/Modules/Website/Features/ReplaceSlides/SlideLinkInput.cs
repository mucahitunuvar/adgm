namespace GenclikMerkezi.Modules.Website.Features.ReplaceSlides;

public sealed record SlideLinkInput(string Kind, Guid? ContentItemId, Guid? ContentTypeId, string? InternalPath, string? ExternalUrl);
