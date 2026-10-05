namespace GenclikMerkezi.Modules.Website.Features.GetFormDefinitionById;

public sealed record FormFieldResponse(
    string Key,
    string Type,
    bool IsRequired,
    int SortOrder,
    int? MinLength,
    int? MaxLength,
    IReadOnlyList<FormFieldOptionResponse> Options,
    DateTime? DateMin,
    DateTime? DateMax,
    IReadOnlyList<string> AllowedFileTypes,
    int? MaxSizeMb,
    IReadOnlyList<FormFieldTranslationResponse> Translations);
