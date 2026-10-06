using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// Faz 4 Görev 4: the event-cancellation fan-out's own recipient projection - email + language only,
// never a full EventRegistration, since the notice carries no other personal data (§1 "sebep metni
// dahil; kişiye özel veri yok").
public sealed record EventRegistrationCancellationRecipient(string Email, LanguageCode LanguageCode);
