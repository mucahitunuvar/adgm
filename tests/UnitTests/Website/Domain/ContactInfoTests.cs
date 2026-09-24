using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContactInfoTests
{
    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var result = ContactInfo.Create("Ankara", "+90 555 000 00 00", "info@example.org", "+90 555 000 00 01", "https://maps.example.com");

        Assert.True(result.IsSuccess);
        Assert.Equal("info@example.org", result.Value.Email);
    }

    [Fact]
    public void Create_WithEmptyOptionalFields_Succeeds()
    {
        var result = ContactInfo.Create(null, null, null, null, null);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.Email);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-at.com")]
    public void Create_WithInvalidEmail_Fails(string email)
    {
        var result = ContactInfo.Create(null, null, email, null, null);

        Assert.True(result.IsFailure);
        Assert.Equal("ContactInfo.InvalidEmail", result.Error.Code);
    }

    [Fact]
    public void Create_WithAddressTooLong_Fails()
    {
        var result = ContactInfo.Create(new string('a', ContactInfo.MaxAddressLength + 1), null, null, null, null);

        Assert.True(result.IsFailure);
        Assert.Equal("ContactInfo.AddressTooLong", result.Error.Code);
    }
}
