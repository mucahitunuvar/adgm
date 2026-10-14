using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBankAccounts;

namespace GenclikMerkezi.UnitTests.Website.Features.UpdateSiteSettingsBankAccounts;

public class UpdateSiteSettingsBankAccountsCommandValidatorTests
{
    private readonly UpdateSiteSettingsBankAccountsCommandValidator _validator = new();

    private static UpdateSiteSettingsBankAccountInput Account(string currency) =>
        new("TR000000000000000000000000", "Banka", "Hesap Sahibi", currency, null, 0, true);

    [Fact]
    public void Validate_WithKnownCurrencyName_Passes()
    {
        var result = _validator.Validate(new UpdateSiteSettingsBankAccountsCommand([1], [Account("TRY")]));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("USD,EUR")] // 1|2 = 3, no defined member - still correctly rejected.
    public void Validate_WithNumericOrCombinedCurrency_Fails(string currency)
    {
        var result = _validator.Validate(new UpdateSiteSettingsBankAccountsCommand([1], [Account(currency)]));

        Assert.False(result.IsValid);
    }
}
