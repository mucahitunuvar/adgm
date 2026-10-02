using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 4 master prompt §4.3: one block within a PageLayout's draft or published list.
// BlockTypeKey/SettingsJson/TextsJson are opaque to the domain - the block type registry (Application
// layer) is the only place that knows how to parse, validate or read references out of them, the same
// separation ContentType.Update leaves cross-field JSON-shape validation to the command handler. This
// type only enforces what it CAN check without that registry: a key is present and translations don't
// collide on language.
public sealed class LayoutBlock : Entity
{
    private readonly List<LayoutBlockTranslation> _translations = [];

    public string BlockTypeKey { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public string SettingsJson { get; private set; } = string.Empty;

    public IReadOnlyList<LayoutBlockTranslation> Translations => _translations.AsReadOnly();

    private LayoutBlock(Guid id, string blockTypeKey, int sortOrder, bool isActive, string settingsJson)
        : base(id)
    {
        BlockTypeKey = blockTypeKey;
        SortOrder = sortOrder;
        IsActive = isActive;
        SettingsJson = settingsJson;
    }

    private LayoutBlock()
    {
    }

    public static Result<LayoutBlock> Create(
        string blockTypeKey, int sortOrder, bool isActive, string settingsJson, IReadOnlyList<LayoutBlockTranslation> translations)
    {
        if (string.IsNullOrWhiteSpace(blockTypeKey))
        {
            return Result.Failure<LayoutBlock>(Error.Validation("LayoutBlock.BlockTypeKeyRequired", "A block type key is required."));
        }

        var duplicateLanguage = translations.GroupBy(t => t.LanguageCode).FirstOrDefault(g => g.Count() > 1);
        if (duplicateLanguage is not null)
        {
            return Result.Failure<LayoutBlock>(Error.Validation(
                "LayoutBlock.DuplicateTranslationLanguage", $"Language '{duplicateLanguage.Key}' appears more than once."));
        }

        var block = new LayoutBlock(Guid.NewGuid(), blockTypeKey, sortOrder, isActive, settingsJson ?? string.Empty);
        block._translations.AddRange(translations);

        return Result.Success(block);
    }

    // Publish/DiscardDraft (PageLayout) copy a block from one list to the other - every copy gets a
    // fresh id, since nothing outside PageLayout ever references a LayoutBlock's id (same rationale as
    // Slide.Id within Slider).
    internal LayoutBlock Clone()
    {
        var clone = new LayoutBlock(Guid.NewGuid(), BlockTypeKey, SortOrder, IsActive, SettingsJson);
        clone._translations.AddRange(_translations.Select(t => t.Clone()));

        return clone;
    }
}
