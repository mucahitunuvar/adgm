using GenclikMerkezi.Modules.Website.Features.GetEventRegistrations;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetEventRegistrations;

public class GetEventRegistrationsQueryHandlerTests
{
    private readonly FakeEventRegistrationRepository _eventRegistrationRepository = new();
    private readonly FakeEventScheduleRepository _eventScheduleRepository = new();

    private GetEventRegistrationsQueryHandler CreateHandler() => new(_eventRegistrationRepository, _eventScheduleRepository);

    [Theory]
    [InlineData("999")]
    [InlineData("Applied,Confirmed")] // 1|2 = 3 = Waitlisted - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedStatus_Fails(string status)
    {
        var result = await CreateHandler().Handle(
            new GetEventRegistrationsQuery(Guid.NewGuid(), status, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("EventRegistration.InvalidStatusFilter", result.Error.Code);
    }
}
