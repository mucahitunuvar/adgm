namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettings;

public sealed record UpdateSiteSettingsBankAccountInput(
    string Iban,
    string BankName,
    string AccountHolder,
    string? Description,
    int SortOrder,
    bool IsActive);
