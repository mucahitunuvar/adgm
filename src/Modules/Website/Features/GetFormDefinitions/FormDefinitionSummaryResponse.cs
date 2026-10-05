namespace GenclikMerkezi.Modules.Website.Features.GetFormDefinitions;

public sealed record FormDefinitionSummaryResponse(
    Guid Id, string Key, string Title, bool IsActive, int DefinitionVersion, int FieldCount, byte[] RowVersion);
