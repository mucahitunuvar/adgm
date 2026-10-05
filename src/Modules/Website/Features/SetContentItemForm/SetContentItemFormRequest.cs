namespace GenclikMerkezi.Modules.Website.Features.SetContentItemForm;

public sealed record SetContentItemFormRequest(byte[] RowVersion, Guid? FormDefinitionId);
