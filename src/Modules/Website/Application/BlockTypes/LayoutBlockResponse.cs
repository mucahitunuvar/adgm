using System.Text.Json;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// Shared by GetHomeLayout/GetContentLayout's responses (§4.3 "GET ... taslak ve yayındaki blokların
// listesi") - Settings/Texts are returned as parsed JSON, not re-typed per block type, the same
// "opaque to everything except the block type registry" choice LayoutBlock itself makes.
public sealed record LayoutBlockResponse(
    string BlockTypeKey, int SortOrder, bool IsActive, JsonElement Settings, IReadOnlyDictionary<string, JsonElement> Texts)
{
    public static LayoutBlockResponse FromDomain(LayoutBlock block) =>
        new(
            block.BlockTypeKey,
            block.SortOrder,
            block.IsActive,
            JsonSerializer.Deserialize<JsonElement>(block.SettingsJson),
            block.Translations.ToDictionary(t => t.LanguageCode.Value, t => JsonSerializer.Deserialize<JsonElement>(t.TextsJson)));
}
