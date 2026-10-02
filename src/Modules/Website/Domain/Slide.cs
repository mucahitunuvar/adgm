using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 2 master prompt §2: one slide of a Slider, replaced as a whole list (Slider.ReplaceSlides)
// the same way MenuItem's whole tree is replaced - never edited node-by-node. DesktopImageMediaId is
// required; MobileImageMediaId and LinkTarget are optional. Visibility (IsActive + publish window) is
// SlideVisibility's job, not this type's - kept here would duplicate ContentItemVisibility's own
// "define the time-window check exactly once" rationale.
public sealed class Slide : Entity
{
    private readonly List<SlideTranslation> _translations = [];

    public Guid DesktopImageMediaId { get; private set; }

    public Guid? MobileImageMediaId { get; private set; }

    public LinkTarget LinkTarget { get; private set; } = LinkTarget.CreateEmpty();

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? PublishAtUtc { get; private set; }

    public DateTime? UnpublishAtUtc { get; private set; }

    public IReadOnlyList<SlideTranslation> Translations => _translations.AsReadOnly();

    private Slide(
        Guid id, Guid desktopImageMediaId, Guid? mobileImageMediaId, LinkTarget linkTarget, int sortOrder, bool isActive,
        DateTime? publishAtUtc, DateTime? unpublishAtUtc)
        : base(id)
    {
        DesktopImageMediaId = desktopImageMediaId;
        MobileImageMediaId = mobileImageMediaId;
        LinkTarget = linkTarget;
        SortOrder = sortOrder;
        IsActive = isActive;
        PublishAtUtc = publishAtUtc;
        UnpublishAtUtc = unpublishAtUtc;
    }

    private Slide()
    {
    }

    public static Result<Slide> Create(
        Guid desktopImageMediaId,
        Guid? mobileImageMediaId,
        LinkTarget linkTarget,
        int sortOrder,
        bool isActive,
        DateTime? publishAtUtc,
        DateTime? unpublishAtUtc,
        IReadOnlyList<SlideTranslation> translations)
    {
        if (desktopImageMediaId == Guid.Empty)
        {
            return Result.Failure<Slide>(Error.Validation("Slide.DesktopImageRequired", "A desktop image is required."));
        }

        if (publishAtUtc is not null && unpublishAtUtc is not null && unpublishAtUtc <= publishAtUtc)
        {
            return Result.Failure<Slide>(Error.Validation(
                "Slide.UnpublishMustBeAfterPublish", "UnpublishAtUtc must be after PublishAtUtc."));
        }

        if (translations.Count == 0)
        {
            return Result.Failure<Slide>(Error.Validation("Slide.TranslationsRequired", "A slide requires at least one translation."));
        }

        // §2 "Kurallar: ButtonLabel varsa LinkTarget zorunlu" - checked across every translation since
        // ButtonLabel is per-language but LinkTarget belongs to the slide as a whole.
        if (translations.Any(t => !string.IsNullOrEmpty(t.ButtonLabel)) && linkTarget.IsEmpty)
        {
            return Result.Failure<Slide>(Error.Validation(
                "Slide.ButtonLabelRequiresLink", "A slide with a button label must also have a link target."));
        }

        var slide = new Slide(
            Guid.NewGuid(), desktopImageMediaId, mobileImageMediaId, linkTarget, sortOrder, isActive, publishAtUtc, unpublishAtUtc);
        slide._translations.AddRange(translations);

        return Result.Success(slide);
    }
}
