using FluentValidation;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBankAccounts;

public sealed class UpdateSiteSettingsBankAccountsCommandValidator : AbstractValidator<UpdateSiteSettingsBankAccountsCommand>
{
    public UpdateSiteSettingsBankAccountsCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();

        RuleForEach(c => c.BankAccounts).ChildRules(account =>
        {
            account.RuleFor(a => a.Iban).NotEmpty();
            account.RuleFor(a => a.BankName).NotEmpty().MaximumLength(BankAccount.MaxBankNameLength);
            account.RuleFor(a => a.AccountHolder).NotEmpty().MaximumLength(BankAccount.MaxAccountHolderLength);
            account.RuleFor(a => a.Currency).Must(c => Enum.TryParse<BankAccountCurrency>(c, out _))
                .WithMessage("Currency must be one of: TRY, USD, EUR.");
            account.RuleFor(a => a.Description).MaximumLength(BankAccount.MaxDescriptionLength);
        });
    }
}
