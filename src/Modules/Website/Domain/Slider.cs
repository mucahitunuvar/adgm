using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 2 master prompt §2: a named, reusable collection of slides (e.g. "home-hero") that a
// future hero-slider block (Görev 4) will reference by Id. Slides are replaced as a whole list
// (ReplaceSlides), the same "whole collection replaced at once" shape Menu.ReplaceItems already uses -
// the admin slide editor always saves every slide together, never one at a time.
public sealed class Slider : AggregateRoot
{
    public const int MaxSlides = 20;

    private readonly List<SliderTranslation> _translations = [];
    private readonly List<Slide> _slides = [];

    public SliderKey Key { get; private set; } = null!;

    public IReadOnlyList<SliderTranslation> Translations => _translations.AsReadOnly();

    public IReadOnlyList<Slide> Slides => _slides.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private Slider(Guid id, SliderKey key, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        Key = key;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private Slider()
    {
    }

    public static Result<Slider> Create(
        string? key, LanguageCode defaultLanguageCode, string? defaultLanguageName, Guid createdByUserId, DateTime createdAtUtc)
    {
        var keyResult = SliderKey.Create(key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<Slider>(keyResult.Error);
        }

        var translationResult = SliderTranslation.Create(defaultLanguageCode, defaultLanguageName);
        if (translationResult.IsFailure)
        {
            return Result.Failure<Slider>(translationResult.Error);
        }

        var slider = new Slider(Guid.NewGuid(), keyResult.Value, createdByUserId, createdAtUtc);
        slider._translations.Add(translationResult.Value);

        return Result.Success(slider);
    }

    // Replaces the slider's entire name-translation set at once (§2 "GET/PUT .../sliders/{id} (ad
    // çevirileri)") - the Application layer is the one that verifies defaultLanguageCode is present,
    // the same separation ReplaceMenuItemsCommandHandler already uses for MenuItem translations.
    public Result ReplaceTranslations(IReadOnlyList<SliderTranslation> translations, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (translations.Count == 0)
        {
            return Result.Failure(Error.Validation("Slider.TranslationsRequired", "A slider requires at least one translation."));
        }

        var duplicateLanguage = translations.GroupBy(t => t.LanguageCode).FirstOrDefault(g => g.Count() > 1);
        if (duplicateLanguage is not null)
        {
            return Result.Failure(Error.Validation(
                "Slider.DuplicateTranslationLanguage", $"Language '{duplicateLanguage.Key}' appears more than once."));
        }

        _translations.Clear();
        _translations.AddRange(translations);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public Result ReplaceSlides(IReadOnlyList<Slide> slides, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (slides.Count > MaxSlides)
        {
            return Result.Failure(Error.Validation("Slider.TooManySlides", $"A slider can have at most {MaxSlides} slides."));
        }

        var duplicateId = slides.GroupBy(s => s.Id).FirstOrDefault(g => g.Count() > 1);
        if (duplicateId is not null)
        {
            return Result.Failure(Error.Validation("Slider.DuplicateSlideId", $"Slide id '{duplicateId.Key}' appears more than once."));
        }

        _slides.Clear();
        _slides.AddRange(slides);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}
