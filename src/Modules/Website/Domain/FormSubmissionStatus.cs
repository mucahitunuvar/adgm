namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 4): every submission starts here. Görev 5 owns the full state machine
// (New -> InReview -> AwaitingInfo -> Approved/Rejected -> Completed, plus archive/reopen) and will
// extend this enum - stored as a string column (HasConversion<string>), so adding members later needs
// no migration.
public enum FormSubmissionStatus
{
    New,
}
