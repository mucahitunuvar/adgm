using GenclikMerkezi.Modules.Identity.Features.RegisterUser;

namespace GenclikMerkezi.UnitTests.Identity.Features.RegisterUser;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    private static RegisterUserCommand ValidCommand() =>
        new("aday@example.com", "Sifre123", "Ahmet", "Yılmaz", "05551234567", "Candidate");

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyFirstName_Fails()
    {
        var command = ValidCommand() with { FirstName = string.Empty };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.FirstName));
    }

    [Fact]
    public void Validate_WithEmptyLastName_Fails()
    {
        var command = ValidCommand() with { LastName = string.Empty };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.LastName));
    }

    [Fact]
    public void Validate_WithNullPhoneNumber_Passes()
    {
        var command = ValidCommand() with { PhoneNumber = null };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
