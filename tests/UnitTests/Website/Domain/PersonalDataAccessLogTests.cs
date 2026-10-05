using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class PersonalDataAccessLogTests
{
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        var result = PersonalDataAccessLog.Create(
            userId, Now, PersonalDataEntityType.FormSubmission, entityId, PersonalDataAccessAction.View, null);

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(Now, result.Value.AccessedAtUtc);
        Assert.Equal(PersonalDataEntityType.FormSubmission, result.Value.EntityType);
        Assert.Equal(entityId, result.Value.EntityId);
        Assert.Equal(PersonalDataAccessAction.View, result.Value.Action);
        Assert.Null(result.Value.Detail);
    }

    [Fact]
    public void Create_WithNullEntityId_Succeeds()
    {
        var result = PersonalDataAccessLog.Create(
            Guid.NewGuid(), Now, PersonalDataEntityType.NewsletterSubscriber, null, PersonalDataAccessAction.Export, "status=Active");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.EntityId);
        Assert.Equal("status=Active", result.Value.Detail);
    }

    [Fact]
    public void Create_WithTooLongDetail_Fails()
    {
        var result = PersonalDataAccessLog.Create(
            Guid.NewGuid(), Now, PersonalDataEntityType.FormSubmission, Guid.NewGuid(), PersonalDataAccessAction.View,
            new string('a', PersonalDataAccessLog.MaxDetailLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal("PersonalDataAccessLog.DetailTooLong", result.Error.Code);
    }
}
