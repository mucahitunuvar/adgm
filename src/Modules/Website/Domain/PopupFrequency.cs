namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 6 master prompt §6: how often the same visitor is shown this popup again. FrequencyDays
// is only meaningful (and only required) for EveryNDays - Popup.Create/Update enforce that pairing.
public enum PopupFrequency
{
    EveryVisit,
    OncePerSession,
    EveryNDays,
}
