namespace GenclikMerkezi.Modules.Website.Features.GetSiteSettings;

public sealed record BankAccountResponse(
    Guid Id, string Iban, string BankName, string AccountHolder, string? Description, int SortOrder, bool IsActive);
