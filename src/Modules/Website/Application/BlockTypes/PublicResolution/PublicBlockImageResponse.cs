namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2: the same small/medium/large/original variant-URL shape every other
// public handler already returns (PublicSliderImageResponse, PublicPartnerLogoResponse,
// PublicContentImageResponse) - redeclared here rather than reused across features, matching this
// module's existing convention of one small image response record per consuming feature/service.
public sealed record PublicBlockImageResponse(string? Small, string? Medium, string? Large, string Original);
