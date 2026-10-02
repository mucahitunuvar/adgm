namespace GenclikMerkezi.Modules.Website.Features.ReplaceSlides;

public sealed record SlideTranslationInput(
    string LanguageCode, string? Eyebrow, string? Title, string? Text, string? ButtonLabel, string? AltTextOverride);
