namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 5): the full submission state machine. Allowed transitions are the exact
// edges ADR-024 §12.2 lists - see FormSubmission.AllowedTransitions - nothing is inferred beyond them
// (e.g. New cannot go directly to Approved, Approved cannot go directly to Rejected). Rejected and
// Completed are the two closing statuses; either can be reopened back to InReview. Stored as a string
// column (HasConversion<string>), so adding members later needs no migration.
public enum FormSubmissionStatus
{
    New,
    InReview,
    AwaitingInfo,
    Approved,
    Rejected,
    Completed,
}
