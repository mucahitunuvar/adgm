using GenclikMerkezi.Modules.Identity.Features.AdminGetAuditLog;

namespace GenclikMerkezi.UnitTests.Identity.Features.AdminGetAuditLog;

public class AdminGetAuditLogQueryValidatorTests
{
    private readonly AdminGetAuditLogQueryValidator _validator = new();

    [Fact]
    public void Validate_WithNullActionType_Passes()
    {
        var result = _validator.Validate(new AdminGetAuditLogQuery(null, null, null, null));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("ManuallyUnlocked,Deactivated")] // 1|2 = 3 = Reactivated.
    public void Validate_WithNumericOrCombinedActionType_Fails(string actionType)
    {
        var result = _validator.Validate(new AdminGetAuditLogQuery(null, actionType, null, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AdminGetAuditLogQuery.ActionType));
    }
}
