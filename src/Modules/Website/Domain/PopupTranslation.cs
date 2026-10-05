using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 6 master prompt §6: one popup's per-language content. Title/Body's rules depend on the
// owning Popup's DisplayMode (passed in by Popup.Create/SetTranslation, which alone knows it) - Modal
// requires a Title and lets Body hold sanitized rich text (sanitizing itself is the command handler's
// job, same split RichTextBlockTypeDefinition uses, §1 "Zengin metin alanları handler'da
// IHtmlContentSanitizer ile temizlenir"); Banner's Title is optional and its Body must already be
// short plain text with no markup by the time it reaches here.
public sealed class PopupTranslation : Entity
{
    public const int MaxTitleLength = 150;
    public const int MaxBannerBodyLength = 300;
    public const int MaxButtonLabelLength = 50;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string? Title { get; private set; }

    public string Body { get; private set; } = string.Empty;

    public string? ButtonLabel { get; private set; }

    private PopupTranslation(Guid id, LanguageCode languageCode, string? title, string body, string? buttonLabel)
        : base(id)
    {
        LanguageCode = languageCode;
        Title = title;
        Body = body;
        ButtonLabel = buttonLabel;
    }

    private PopupTranslation()
    {
    }

    public static Result<PopupTranslation> Create(
        LanguageCode languageCode, PopupDisplayMode displayMode, string? title, string? body, string? buttonLabel)
    {
        var titleResult = NormalizeTitle(displayMode, title);
        if (titleResult.IsFailure)
        {
            return Result.Failure<PopupTranslation>(titleResult.Error);
        }

        var bodyResult = NormalizeBody(displayMode, body);
        if (bodyResult.IsFailure)
        {
            return Result.Failure<PopupTranslation>(bodyResult.Error);
        }

        var buttonLabelResult = NormalizeButtonLabel(buttonLabel);
        if (buttonLabelResult.IsFailure)
        {
            return Result.Failure<PopupTranslation>(buttonLabelResult.Error);
        }

        return Result.Success(new PopupTranslation(Guid.NewGuid(), languageCode, titleResult.Value, bodyResult.Value, buttonLabelResult.Value));
    }

    internal Result Update(PopupDisplayMode displayMode, string? title, string? body, string? buttonLabel)
    {
        var titleResult = NormalizeTitle(displayMode, title);
        if (titleResult.IsFailure)
        {
            return titleResult;
        }

        var bodyResult = NormalizeBody(displayMode, body);
        if (bodyResult.IsFailure)
        {
            return bodyResult;
        }

        var buttonLabelResult = NormalizeButtonLabel(buttonLabel);
        if (buttonLabelResult.IsFailure)
        {
            return buttonLabelResult;
        }

        Title = titleResult.Value;
        Body = bodyResult.Value;
        ButtonLabel = buttonLabelResult.Value;

        return Result.Success();
    }

    private static Result<string?> NormalizeTitle(PopupDisplayMode displayMode, string? title)
    {
        var trimmed = (title ?? string.Empty).Trim();

        if (trimmed.Length == 0)
        {
            return displayMode == PopupDisplayMode.Modal
                ? Result.Failure<string?>(Error.Validation("PopupTranslation.TitleRequired", "A title is required for a Modal popup."))
                : Result.Success<string?>(null);
        }

        if (trimmed.Length > MaxTitleLength)
        {
            return Result.Failure<string?>(Error.Validation(
                "PopupTranslation.TitleTooLong", $"Title must be at most {MaxTitleLength} characters."));
        }

        return Result.Success<string?>(trimmed);
    }

    // §6 "Body (sanitize; Banner'da maks 300 karakter düz metin, HTML yok)" - the "no HTML" half is
    // checked here (a '<' anywhere means markup slipped through); actually stripping/allowing markup
    // for Modal is the command handler's job via IHtmlContentSanitizer, before this ever runs.
    private static Result<string> NormalizeBody(PopupDisplayMode displayMode, string? body)
    {
        var trimmed = (body ?? string.Empty).Trim();

        if (trimmed.Length == 0)
        {
            return Result.Failure<string>(Error.Validation("PopupTranslation.BodyRequired", "A body is required."));
        }

        if (displayMode == PopupDisplayMode.Banner)
        {
            if (trimmed.Length > MaxBannerBodyLength)
            {
                return Result.Failure<string>(Error.Validation(
                    "PopupTranslation.BannerBodyTooLong", $"A banner's body must be at most {MaxBannerBodyLength} characters."));
            }

            if (trimmed.Contains('<'))
            {
                return Result.Failure<string>(Error.Validation(
                    "PopupTranslation.BannerBodyMustBePlainText", "A banner's body must be plain text; it cannot contain HTML."));
            }
        }

        return Result.Success(trimmed);
    }

    private static Result<string?> NormalizeButtonLabel(string? buttonLabel)
    {
        var trimmed = buttonLabel?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return Result.Success<string?>(null);
        }

        if (trimmed.Length > MaxButtonLabelLength)
        {
            return Result.Failure<string?>(Error.Validation(
                "PopupTranslation.ButtonLabelTooLong", $"Button label must be at most {MaxButtonLabelLength} characters."));
        }

        return Result.Success<string?>(trimmed);
    }
}
