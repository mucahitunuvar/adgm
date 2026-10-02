using System.Text.Json;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// §4.3 "PUT .../draft - taslak blok listesinin tamamı": one block as the admin editor sends/receives
// it. Settings/Texts stay as raw JsonElement here - their shape depends entirely on BlockTypeKey, so
// only the matching IBlockTypeDefinition (looked up by LayoutBlockInputProcessor) can parse them.
public sealed record LayoutBlockInput(
    string BlockTypeKey, int SortOrder, bool IsActive, JsonElement Settings, IReadOnlyDictionary<string, JsonElement>? Texts = null);
