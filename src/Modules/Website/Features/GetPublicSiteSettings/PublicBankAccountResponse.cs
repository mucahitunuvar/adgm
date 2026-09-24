namespace GenclikMerkezi.Modules.Website.Features.GetPublicSiteSettings;

public sealed record PublicBankAccountResponse(string Iban, string BankName, string AccountHolder, string? Description, int SortOrder);
