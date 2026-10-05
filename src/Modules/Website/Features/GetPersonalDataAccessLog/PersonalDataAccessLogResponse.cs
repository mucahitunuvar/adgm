namespace GenclikMerkezi.Modules.Website.Features.GetPersonalDataAccessLog;

public sealed record PersonalDataAccessLogResponse(
    Guid Id, Guid UserId, DateTime AccessedAtUtc, string EntityType, Guid? EntityId, string Action, string? Detail);
