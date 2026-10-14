using GenclikMerkezi.Modules.Identity.Features.ChangeUserRole;

namespace GenclikMerkezi.UnitTests.Identity.Features.ChangeUserRole;

public class ChangeUserRoleCommandValidatorTests
{
    private readonly ChangeUserRoleCommandValidator _validator = new();

    [Fact]
    public void Validate_WithKnownRoleName_Passes()
    {
        var result = _validator.Validate(new ChangeUserRoleCommand(Guid.NewGuid(), "Admin"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("Employer,Candidate")] // 1|2 = 3 = CareerAdvisor - Enum.TryParse would silently alias to this.
    public void Validate_WithNumericOrCombinedRole_Fails(string newRole)
    {
        var result = _validator.Validate(new ChangeUserRoleCommand(Guid.NewGuid(), newRole));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangeUserRoleCommand.NewRole));
    }
}
