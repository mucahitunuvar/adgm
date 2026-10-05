namespace GenclikMerkezi.Modules.Website.Application.Forms.PublicResolution;

public sealed record PublicFormFieldResponse(
    string Key,
    string Type,
    string Label,
    string? Placeholder,
    string? HelpText,
    bool IsRequired,
    int? MinLength,
    int? MaxLength,
    IReadOnlyList<PublicFormFieldOptionResponse> Options,
    DateTime? DateMin,
    DateTime? DateMax,
    IReadOnlyList<string> AllowedFileTypes,
    int? MaxSizeMb);
