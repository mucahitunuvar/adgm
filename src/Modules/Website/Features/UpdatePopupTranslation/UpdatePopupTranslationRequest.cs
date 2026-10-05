namespace GenclikMerkezi.Modules.Website.Features.UpdatePopupTranslation;

public sealed record UpdatePopupTranslationRequest(byte[] RowVersion, string? Title, string? Body, string? ButtonLabel);
