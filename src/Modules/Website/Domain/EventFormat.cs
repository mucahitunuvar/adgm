namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §11.1 (Faz 4 Görev 1): whether the event happens in person, fully online, or both -
// OnlineLink is required exactly when Online or Hybrid (EventSchedule.NormalizeOnlineLink).
public enum EventFormat
{
    InPerson,
    Online,
    Hybrid,
}
