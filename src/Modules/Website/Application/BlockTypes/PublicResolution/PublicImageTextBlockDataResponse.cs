namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2: image-text's `data` - Href is null when Settings carried no link
// or it did not resolve (button hidden, block stays, §5.2-hiding "tekil link'lerde ilgili buton ...
// gizlenir").
public sealed record PublicImageTextBlockDataResponse(PublicBlockImageResponse Image, string? Href);
