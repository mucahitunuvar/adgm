namespace GenclikMerkezi.Modules.Website.Features.SetFormFields;

public sealed record FormFieldInput(
    string? Key,
    string? Type,
    bool IsRequired,
    int SortOrder,
    int? MinLength,
    int? MaxLength,
    IReadOnlyList<FormFieldOptionInput> Options,
    DateTime? DateMin,
    DateTime? DateMax,
    IReadOnlyList<string> AllowedFileTypes,
    int? MaxSizeMb,
    IReadOnlyList<FormFieldTranslationInput> Translations);
