namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBankAccounts;

public sealed record UpdateSiteSettingsBankAccountInput(
    string Iban,
    string BankName,
    string AccountHolder,
    string Currency,
    string? Description,
    int SortOrder,
    bool IsActive);
