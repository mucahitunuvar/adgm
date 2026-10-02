namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// Faz 2 Görev 4 master prompt §4.2: "Tüm link alanları LinkTarget'tır" - the JSON-serializable shape a
// block's Settings/Texts record carries for a link field, mirroring SlideLinkInput/MenuItemLinkInput's
// own request-DTO shape. LinkTargetDtoMapper turns this into the real domain LinkTarget value object.
public sealed record LinkTargetDto(string Kind, Guid? ContentItemId, Guid? ContentTypeId, string? InternalPath, string? ExternalUrl);
