namespace GenclikMerkezi.Modules.Website.Application.Sliders;

// §2 "Link'i çözümlenemeyen slide'ın butonu gizlenir, slide gösterilir": ButtonLabel/ButtonHref are
// both null whenever the translation had no button label, or it did but LinkTarget did not resolve -
// never a label with no href or vice versa.
public sealed record PublicSliderSlideResponse(
    Guid Id,
    string? Eyebrow,
    string Title,
    string? Text,
    PublicSliderImageResponse Desktop,
    PublicSliderImageResponse? Mobile,
    string AltText,
    string? ButtonLabel,
    string? ButtonHref);
