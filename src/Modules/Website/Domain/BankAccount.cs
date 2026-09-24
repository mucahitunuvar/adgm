using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13: one bank account listed on the (currently inactive) "Destek Ol" donation page. Owned
// by SiteSettings and always replaced as a whole list (SiteSettings.ReplaceBankAccounts), same
// reasoning as SocialLink.
public sealed class BankAccount : Entity
{
    public const int MaxBankNameLength = 150;
    public const int MaxAccountHolderLength = 150;
    public const int MaxDescriptionLength = 300;

    public Iban Iban { get; private set; } = null!;

    public string BankName { get; private set; } = string.Empty;

    public string AccountHolder { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    private BankAccount(Guid id, Iban iban, string bankName, string accountHolder, string? description, int sortOrder, bool isActive)
        : base(id)
    {
        Iban = iban;
        BankName = bankName;
        AccountHolder = accountHolder;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
    }

    private BankAccount()
    {
    }

    public static BankAccount Create(Iban iban, string bankName, string accountHolder, string? description, int sortOrder, bool isActive) =>
        new(
            Guid.NewGuid(), iban, bankName.Trim(), accountHolder.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(), sortOrder, isActive);
}
