using GenclikMerkezi.Modules.Identity.Features.CreateStaffUser;

namespace GenclikMerkezi.UnitTests.Identity.Features.CreateStaffUser;

public class CreateStaffUserCommandValidatorTests
{
    private readonly CreateStaffUserCommandValidator _validator = new();

    private static CreateStaffUserCommand ValidCommand() =>
        new("danisman@example.com", "Sifre123", "Ayşe", "Kaya", "05551234567", "CareerAdvisor");

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Candidate")]
    [InlineData("Employer")]
    [InlineData("Admin")]
    [InlineData("")]
    public void Validate_WithNonStaffRole_Fails(string role)
    {
        var command = ValidCommand() with { Role = role };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateStaffUserCommand.Role));
    }

    [Fact]
    public void Validate_WithEmptyFirstName_Fails()
    {
        var command = ValidCommand() with { FirstName = string.Empty };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateStaffUserCommand.FirstName));
    }
}
