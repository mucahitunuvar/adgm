namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §12.3 (Faz 3 Görev 1): the common anonymous-write fields IPublicSubmissionGuard checks -
// shared by the form submission (Görev 4), newsletter subscription (Görev 6) and cookie consent
// (Görev 7) requests, each of which embeds this alongside its own fields. Website is the honeypot: a
// field named to look like a normal one, hidden from real visitors by the frontend's CSS/markup, that
// only an automated client fills in.
public sealed record PublicSubmissionGuardRequest(string? SubmissionToken, string? TurnstileToken, string? Website);
