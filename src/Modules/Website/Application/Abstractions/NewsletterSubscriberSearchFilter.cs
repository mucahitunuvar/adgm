namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §14 (Faz 3 Görev 6): the admin list's filters - GetNewsletterSubscribersQueryHandler builds
// this from the query's own fields, the same split FormSubmissionSearchFilter already follows. Search
// matches the email (contains).
public sealed record NewsletterSubscriberSearchFilter(string? Status, string? Language, string? Search);
