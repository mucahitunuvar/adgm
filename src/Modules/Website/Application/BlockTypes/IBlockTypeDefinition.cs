using System.Text.Json;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// §4.1: one block type's full contract - key, where it may be used, how to parse/validate its raw
// JSON, which entities it references, and how to describe its own shape for the block-types catalog
// endpoint. Non-generic so IBlockTypeRegistry can hold every block type (each with its own distinct
// Settings/Texts record pair) in a single collection.
public interface IBlockTypeDefinition
{
    string Key { get; }

    IReadOnlyList<PageLayoutTargetKind> AllowedTargets { get; }

    bool HasTexts { get; }

    Result<ParsedBlockContent> ParseAndValidate(
        JsonElement settingsElement, IReadOnlyDictionary<LanguageCode, JsonElement> textsByLanguage, LanguageCode defaultLanguageCode);

    // Best-effort reference extraction for a block that was already validated and stored - used by
    // PageLayoutReferenceScanner (SliderUsageChecker/VideoUsageChecker/LayoutMediaUsageProvider's
    // deletion guards), which must not fail just because, say, the default language changed after
    // the block was saved. Unlike ParseAndValidate, this neither enforces the default-language-texts
    // rule nor runs ValidateSettings/ValidateTexts - a block that no longer parses contributes no
    // references rather than throwing.
    BlockReferenceSet ExtractReferences(JsonElement settingsElement, IReadOnlyDictionary<LanguageCode, JsonElement> textsByLanguage);

    IReadOnlyList<BlockTypeFieldDescriptor> DescribeSettingsFields();

    IReadOnlyList<BlockTypeFieldDescriptor> DescribeTextsFields();
}
