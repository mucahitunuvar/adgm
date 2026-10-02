namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// §4.2: `rich-text` is the one block type with no language-independent settings at all ("Ayarlar:
// —"). A shared empty marker keeps BlockTypeDefinition<TSettings, TTexts> total over every catalog
// entry without a block-type-specific "no settings" special case.
public sealed record EmptyBlockSettings;
