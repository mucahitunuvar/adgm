using GenclikMerkezi.Modules.Website.Features.ExportEventRegistrations;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.ExportEventRegistrations;

public class ExportEventRegistrationsQueryHandlerTests
{
    private readonly FakeEventRegistrationRepository _eventRegistrationRepository = new();
    private readonly FakeEventScheduleRepository _eventScheduleRepository = new();

    private ExportEventRegistrationsQueryHandler CreateHandler() => new(_eventRegistrationRepository, _eventScheduleRepository);

    [Theory]
    [InlineData("999")]
    [InlineData("Applied,Confirmed")] // 1|2 = 3 = Waitlisted - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedStatus_Fails(string status)
    {
        var result = await CreateHandler().Handle(new ExportEventRegistrationsQuery(Guid.NewGuid(), status), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("EventRegistration.InvalidStatusFilter", result.Error.Code);
    }
}
