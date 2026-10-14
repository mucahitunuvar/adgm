using GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;

namespace GenclikMerkezi.UnitTests.Identity.Features.AdminGetUsers;

public class AdminGetUsersQueryValidatorTests
{
    private readonly AdminGetUsersQueryValidator _validator = new();

    [Fact]
    public void Validate_WithNullFilters_Passes()
    {
        var result = _validator.Validate(new AdminGetUsersQuery(null, null, null, null, null));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("Employer,Candidate")] // 1|2 = 3 = CareerAdvisor.
    public void Validate_WithNumericOrCombinedRole_Fails(string role)
    {
        var result = _validator.Validate(new AdminGetUsersQuery(null, role, null, null, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AdminGetUsersQuery.Role));
    }

    [Theory]
    [InlineData("999")]
    [InlineData("Active,Disabled")] // 1|4 = 5 = Deleted.
    public void Validate_WithNumericOrCombinedStatus_Fails(string status)
    {
        var result = _validator.Validate(new AdminGetUsersQuery(null, null, status, null, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AdminGetUsersQuery.Status));
    }
}
