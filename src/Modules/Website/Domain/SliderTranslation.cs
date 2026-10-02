using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 2 master prompt §2: a Slider's per-language admin-facing name ("Ana Sayfa Hero") - shown
// only in the editor, never on the public site (slide content, not slider name, is what the public
// response exposes). Mirrors VideoTranslation's shape and upsert semantics.
public sealed class SliderTranslation : Entity
{
    public const int MaxNameLength = 150;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    private SliderTranslation(Guid id, LanguageCode languageCode, string name)
        : base(id)
    {
        LanguageCode = languageCode;
        Name = name;
    }

    private SliderTranslation()
    {
    }

    public static Result<SliderTranslation> Create(LanguageCode languageCode, string? name)
    {
        var normalized = (name ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxNameLength)
        {
            return Result.Failure<SliderTranslation>(Error.Validation(
                "SliderTranslation.NameInvalid", $"Name is required and must be at most {MaxNameLength} characters."));
        }

        return Result.Success(new SliderTranslation(Guid.NewGuid(), languageCode, normalized));
    }
}
