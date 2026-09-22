namespace GenclikMerkezi.Modules.Employment.Features.EndEmployment;

public sealed record EndEmploymentRequest(string DepartureReason, DateTime EndDateUtc);
