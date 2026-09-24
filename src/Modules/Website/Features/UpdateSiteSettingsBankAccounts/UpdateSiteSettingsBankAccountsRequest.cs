namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBankAccounts;

public sealed record UpdateSiteSettingsBankAccountsRequest(byte[] RowVersion, IReadOnlyList<UpdateSiteSettingsBankAccountInput> BankAccounts);
