namespace GenclikMerkezi.Modules.Website.Features.UpdateFormDefinitionTranslation;

public sealed record UpdateFormDefinitionTranslationRequest(
    byte[] RowVersion, string? Title, string? Description, string? SuccessMessage, string? SubmitButtonLabel);
