namespace GenclikMerkezi.Modules.Website.Features.GetSiteSettings;

public sealed record BankAccountResponse(
    Guid Id, string Iban, string BankName, string AccountHolder, string Currency, string? Description, int SortOrder, bool IsActive);
