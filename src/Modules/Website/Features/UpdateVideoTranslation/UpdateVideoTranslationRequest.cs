namespace GenclikMerkezi.Modules.Website.Features.UpdateVideoTranslation;

public sealed record UpdateVideoTranslationRequest(byte[] RowVersion, string? Title, string? Description);
