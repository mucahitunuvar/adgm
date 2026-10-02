using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 4 master prompt §4.3: one language's serialized Texts record for a LayoutBlock.
// TextsJson's shape is validated by the owning block type's definition (Application layer) before
// this is ever created - this type only carries the already-validated JSON, the same "domain stores
// the opaque blob, Application validates its shape" split LayoutBlock itself uses for SettingsJson.
public sealed class LayoutBlockTranslation : Entity
{
    public LanguageCode LanguageCode { get; private set; } = null!;

    public string TextsJson { get; private set; } = string.Empty;

    private LayoutBlockTranslation(Guid id, LanguageCode languageCode, string textsJson)
        : base(id)
    {
        LanguageCode = languageCode;
        TextsJson = textsJson;
    }

    private LayoutBlockTranslation()
    {
    }

    public static LayoutBlockTranslation Create(LanguageCode languageCode, string textsJson) =>
        new(Guid.NewGuid(), languageCode, textsJson);

    // Publish/DiscardDraft copy a block from one block list (draft/published) to the other - each
    // copy needs its own fresh id, the same "replaced wholesale, ids never referenced from outside"
    // rationale Slide/MenuItem already use within their own aggregates.
    internal LayoutBlockTranslation Clone() => new(Guid.NewGuid(), LanguageCode, TextsJson);
}
