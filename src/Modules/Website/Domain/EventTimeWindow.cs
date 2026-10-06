namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §17 (Faz 4 Görev 2): GetPublicEvents' "when" filter - Upcoming (default) means "not yet
// concluded" (EndsAtUtc >= now, so an in-progress event still shows), Past means already concluded
// (EndsAtUtc < now), All applies no time filter. Also decides sort direction: ascending for
// Upcoming/All, descending for Past (§ "past sıralaması azalan").
public enum EventTimeWindow
{
    Upcoming,
    Past,
    All,
}
