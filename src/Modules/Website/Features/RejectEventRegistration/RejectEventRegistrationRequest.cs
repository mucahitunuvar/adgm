namespace GenclikMerkezi.Modules.Website.Features.RejectEventRegistration;

public sealed record RejectEventRegistrationRequest(byte[] RowVersion, string? Reason);
