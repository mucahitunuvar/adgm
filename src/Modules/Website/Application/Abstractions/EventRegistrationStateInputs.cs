namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §17 (Faz 4 Görev 2): the exact inputs EventRegistrationStateResolver.Resolve needs, fetched
// as a lightweight projection (no translations, no aggregate load) and never cached - §1 "kontenjan/
// durum alanları cache'lenmez, her istekte hafif bir projection ile hesaplanır". Used by both the
// public events list (one query for a whole page of ids) and the public content detail endpoint (one
// query for a single id).
public sealed record EventRegistrationStateInputs(
    bool IsCancelled,
    bool RegistrationEnabled,
    DateTime? RegistrationOpensAtUtc,
    DateTime? RegistrationClosesAtUtc,
    DateTime StartsAtUtc,
    int? Capacity,
    int ConfirmedCount,
    bool WaitlistEnabled);
