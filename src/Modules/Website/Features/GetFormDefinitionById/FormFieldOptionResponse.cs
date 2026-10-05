namespace GenclikMerkezi.Modules.Website.Features.GetFormDefinitionById;

public sealed record FormFieldOptionResponse(string Key, IReadOnlyList<FormFieldOptionTranslationResponse> Translations);
