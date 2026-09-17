namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;

public sealed record CreateLookupItemRequest(string Code, string DisplayName, int SortOrder);
