using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// IBlockTypeDefinition.ParseAndValidate's result: the canonical (re-serialized, camelCase) JSON for
// SettingsJson/TextsJson columns plus the entity references the outer PageLayoutReferenceValidator
// still owes a database round trip to confirm.
public sealed record ParsedBlockContent(
    string SettingsJson, IReadOnlyDictionary<LanguageCode, string> TextsJsonByLanguage, BlockReferenceSet References);
