namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §14 (Faz 3 Görev 6): the CSV export's row shape - exactly the three columns the master
// prompt specifies ("email, language, confirmedAtUtc"), nothing wider.
public sealed record NewsletterSubscriberExportItem(string Email, string LanguageCode, DateTime? ConfirmedAtUtc);
