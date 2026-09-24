namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13 / Görev 6: currency of one BankAccount's balance/IBAN, shown next to the account on
// the (currently inactive) "Destek Ol" donation page. Named after the literal ISO 4217 codes (not
// PascalCase) since that is the form the master prompt, the UI and any bank statement use.
public enum BankAccountCurrency
{
    TRY,
    USD,
    EUR,
}
