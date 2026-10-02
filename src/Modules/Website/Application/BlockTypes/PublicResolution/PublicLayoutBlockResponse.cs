using System.Text.Json;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2: "Her blok: type, settings (yalnızca frontend'e gereken alanlar;
// iç ID'ler yerine çözümlenmiş veri), texts (istenen dil), data (çözümlenmiş veri)". Settings/Texts are
// null for the block types whose every field is either UI-irrelevant or fully represented in Data
// already (see PublicPageLayoutResolver's per-type remarks); when present, they are the block's raw
// stored JSON for the resolved language, exactly as LayoutBlockResponse already returns them to admins.
public sealed record PublicLayoutBlockResponse(string Type, JsonElement? Settings, JsonElement? Texts, object? Data);
