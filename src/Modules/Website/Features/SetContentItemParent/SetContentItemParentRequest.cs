namespace GenclikMerkezi.Modules.Website.Features.SetContentItemParent;

public sealed record SetContentItemParentRequest(byte[] RowVersion, Guid? ParentId);
