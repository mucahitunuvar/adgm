namespace GenclikMerkezi.Modules.Website.Features.UpdatePartnerTranslation;

public sealed record UpdatePartnerTranslationRequest(byte[] RowVersion, string? Name, string? Description);
