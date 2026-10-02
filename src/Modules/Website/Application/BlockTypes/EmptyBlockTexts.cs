namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// §4.2: `hero-slider` is the one block type with no per-language texts at all ("Metinler: —"). This
// shared marker is also how BlockTypeDefinition.HasTexts decides whether a block even needs a
// default-language Texts entry.
public sealed record EmptyBlockTexts;
