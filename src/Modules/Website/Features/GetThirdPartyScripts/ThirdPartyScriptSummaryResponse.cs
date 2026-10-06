namespace GenclikMerkezi.Modules.Website.Features.GetThirdPartyScripts;

public sealed record ThirdPartyScriptSummaryResponse(
    Guid Id,
    string Provider,
    string Category,
    string Placement,
    string Name,
    int SortOrder,
    bool IsActive,
    byte[] RowVersion);
