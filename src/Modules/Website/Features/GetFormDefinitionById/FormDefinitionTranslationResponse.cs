namespace GenclikMerkezi.Modules.Website.Features.GetFormDefinitionById;

public sealed record FormDefinitionTranslationResponse(
    string LanguageCode, string Title, string Description, string SuccessMessage, string SubmitButtonLabel);
