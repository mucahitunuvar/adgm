namespace GenclikMerkezi.Modules.Website.Features.AssignFormSubmission;

public sealed record AssignFormSubmissionRequest(byte[] RowVersion, Guid? AssignedToUserId);
