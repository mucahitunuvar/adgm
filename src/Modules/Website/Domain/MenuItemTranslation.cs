using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 1 master prompt §1.2: a MenuItem's per-language display label. An item with no
// translation in the requested language is hidden entirely from that language's public menu (Görev
// 1's own rule - no fallback to the default language), the same "no fallback" rule ADR-024 §3 already
// applies to content translations.
public sealed class MenuItemTranslation : Entity
{
    public const int MaxLabelLength = 100;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Label { get; private set; } = string.Empty;

    private MenuItemTranslation(Guid id, LanguageCode languageCode, string label)
        : base(id)
    {
        LanguageCode = languageCode;
        Label = label;
    }

    private MenuItemTranslation()
    {
    }

    public static Result<MenuItemTranslation> Create(LanguageCode languageCode, string? label)
    {
        var normalized = (label ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxLabelLength)
        {
            return Result.Failure<MenuItemTranslation>(Error.Validation(
                "MenuItemTranslation.LabelInvalid", $"Label is required and must be at most {MaxLabelLength} characters."));
        }

        return Result.Success(new MenuItemTranslation(Guid.NewGuid(), languageCode, normalized));
    }
}
