namespace GenclikMerkezi.Modules.Website.Features.GetContentTypes;

public sealed record ContentTypeSummaryResponse(
    Guid Id,
    string Key,
    string DefaultLanguageName,
    string ListTemplate,
    string DetailTemplate,
    string SortMode,
    bool IsActive,
    int SortOrder,
    int ContentCount,
    byte[] RowVersion);
