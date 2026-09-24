namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

public sealed record PublicBankAccountResponse(string Iban, string BankName, string AccountHolder, string? Description, int SortOrder);
