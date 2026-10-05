namespace GenclikMerkezi.Modules.Website.Features.ChangeFormSubmissionStatus;

public sealed record ChangeFormSubmissionStatusRequest(byte[] RowVersion, string Status);
