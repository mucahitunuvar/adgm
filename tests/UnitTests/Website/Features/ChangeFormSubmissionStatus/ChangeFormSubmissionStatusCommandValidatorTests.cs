using GenclikMerkezi.Modules.Website.Features.ChangeFormSubmissionStatus;

namespace GenclikMerkezi.UnitTests.Website.Features.ChangeFormSubmissionStatus;

public class ChangeFormSubmissionStatusCommandValidatorTests
{
    private readonly ChangeFormSubmissionStatusCommandValidator _validator = new();

    [Fact]
    public void Validate_WithKnownStatusName_Passes()
    {
        var result = _validator.Validate(new ChangeFormSubmissionStatusCommand(Guid.NewGuid(), [1], "InReview"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("InReview,AwaitingInfo")] // 1|2 = 3 = Approved - the scenario the audit flagged.
    public void Validate_WithNumericOrCombinedStatus_Fails(string status)
    {
        var result = _validator.Validate(new ChangeFormSubmissionStatusCommand(Guid.NewGuid(), [1], status));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangeFormSubmissionStatusCommand.Status));
    }
}
