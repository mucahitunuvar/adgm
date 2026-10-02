using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 2 master prompt §2: one slide's per-language content. Title is the only required field
// (mirrors every other Website translation's "at least a title/label" shape); Eyebrow, Text,
// ButtonLabel and AltTextOverride are all optional. Slide.Create enforces the cross-field
// "ButtonLabel var ise LinkTarget zorunlu" rule, since LinkTarget lives on the owning Slide, not here.
public sealed class SlideTranslation : Entity
{
    public const int MaxEyebrowLength = 100;
    public const int MaxTitleLength = 150;
    public const int MaxTextLength = 400;
    public const int MaxButtonLabelLength = 50;
    public const int MaxAltTextOverrideLength = 250;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string? Eyebrow { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Text { get; private set; }

    public string? ButtonLabel { get; private set; }

    public string? AltTextOverride { get; private set; }

    private SlideTranslation(
        Guid id, LanguageCode languageCode, string? eyebrow, string title, string? text, string? buttonLabel, string? altTextOverride)
        : base(id)
    {
        LanguageCode = languageCode;
        Eyebrow = eyebrow;
        Title = title;
        Text = text;
        ButtonLabel = buttonLabel;
        AltTextOverride = altTextOverride;
    }

    private SlideTranslation()
    {
    }

    public static Result<SlideTranslation> Create(
        LanguageCode languageCode, string? eyebrow, string? title, string? text, string? buttonLabel, string? altTextOverride)
    {
        var eyebrowResult = NormalizeOptional(eyebrow, MaxEyebrowLength, "SlideTranslation.EyebrowTooLong", "Eyebrow");
        if (eyebrowResult.IsFailure)
        {
            return Result.Failure<SlideTranslation>(eyebrowResult.Error);
        }

        var normalizedTitle = (title ?? string.Empty).Trim();
        if (normalizedTitle.Length == 0 || normalizedTitle.Length > MaxTitleLength)
        {
            return Result.Failure<SlideTranslation>(Error.Validation(
                "SlideTranslation.TitleInvalid", $"Title is required and must be at most {MaxTitleLength} characters."));
        }

        var textResult = NormalizeOptional(text, MaxTextLength, "SlideTranslation.TextTooLong", "Text");
        if (textResult.IsFailure)
        {
            return Result.Failure<SlideTranslation>(textResult.Error);
        }

        var buttonLabelResult = NormalizeOptional(buttonLabel, MaxButtonLabelLength, "SlideTranslation.ButtonLabelTooLong", "Button label");
        if (buttonLabelResult.IsFailure)
        {
            return Result.Failure<SlideTranslation>(buttonLabelResult.Error);
        }

        var altTextResult = NormalizeOptional(
            altTextOverride, MaxAltTextOverrideLength, "SlideTranslation.AltTextOverrideTooLong", "Alt text override");
        if (altTextResult.IsFailure)
        {
            return Result.Failure<SlideTranslation>(altTextResult.Error);
        }

        return Result.Success(new SlideTranslation(
            Guid.NewGuid(), languageCode, eyebrowResult.Value, normalizedTitle, textResult.Value, buttonLabelResult.Value,
            altTextResult.Value));
    }

    private static Result<string?> NormalizeOptional(string? value, int maxLength, string errorCode, string fieldLabel)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return Result.Success<string?>(null);
        }

        if (trimmed.Length > maxLength)
        {
            return Result.Failure<string?>(Error.Validation(errorCode, $"{fieldLabel} must be at most {maxLength} characters."));
        }

        return Result.Success<string?>(trimmed);
    }
}
