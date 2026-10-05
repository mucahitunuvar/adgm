namespace GenclikMerkezi.Modules.Website.Features.GetFormDefinitionById;

public sealed record FormFieldTranslationResponse(string LanguageCode, string Label, string? Placeholder, string? HelpText);
