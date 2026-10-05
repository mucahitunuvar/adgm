namespace GenclikMerkezi.Modules.Website.Features.SetFormFields;

public sealed record FormFieldOptionInput(string? Key, IReadOnlyList<FormFieldOptionTranslationInput> Translations);
