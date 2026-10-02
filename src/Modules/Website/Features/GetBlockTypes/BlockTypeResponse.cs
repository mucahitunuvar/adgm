using GenclikMerkezi.Modules.Website.Application.BlockTypes;

namespace GenclikMerkezi.Modules.Website.Features.GetBlockTypes;

// §4.1 "GET /api/v1/admin/website/block-types - her blok tipi için anahtar, kullanılabildiği hedefler
// ve alan açıklamaları ... tek kaynaktan üretilmeli" - SettingsFields/TextsFields come straight from
// BlockTypeFieldDescriber's reflection over the block type's own Settings/Texts record, never a
// hand-written second schema.
public sealed record BlockTypeResponse(
    string Key,
    IReadOnlyList<string> AllowedTargets,
    IReadOnlyList<BlockTypeFieldDescriptor> SettingsFields,
    IReadOnlyList<BlockTypeFieldDescriptor> TextsFields);
