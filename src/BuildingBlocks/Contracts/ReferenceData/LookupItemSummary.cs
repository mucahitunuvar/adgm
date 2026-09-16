namespace GenclikMerkezi.Contracts.ReferenceData;

public sealed record LookupItemSummary(Guid Id, string Code, string DisplayName, bool IsActive, int SortOrder);
