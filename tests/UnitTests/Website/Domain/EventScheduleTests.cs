using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class EventScheduleTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly Guid ContentItemId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 24, 12, 0, 0, DateTimeKind.Utc);

    private static Result<EventSchedule> CreateSchedule(
        DateTime? startsAtUtc = null,
        DateTime? endsAtUtc = null,
        EventFormat format = EventFormat.InPerson,
        string? onlineLink = null,
        int? capacity = 10,
        DateTime? registrationClosesAtUtc = null,
        int? minAge = null,
        int? maxAge = null) =>
        EventSchedule.Create(
            ContentItemId, startsAtUtc ?? Now.AddDays(10), endsAtUtc ?? Now.AddDays(10).AddHours(2), format, onlineLink, capacity,
            registrationEnabled: true, registrationOpensAtUtc: null, registrationClosesAtUtc, minAge, maxAge, autoConfirm: true,
            waitlistEnabled: true, Tr, "Gençlik Merkezi Salonu", "Örnek Adres No:1", "Ücretsiz", "Eğitmen Adı", "<p>Program</p>",
            "Erişilebilir", UserId, Now);

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var result = CreateSchedule();

        Assert.True(result.IsSuccess);
        Assert.Equal(ContentItemId, result.Value.ContentItemId);
        Assert.Equal(0, result.Value.ConfirmedCount);
        Assert.Equal(0, result.Value.WaitlistedCount);
        Assert.False(result.Value.IsCancelled);
        Assert.Single(result.Value.Translations);
    }

    [Theory]
    [InlineData(0)] // EndsAtUtc == StartsAtUtc
    [InlineData(-1)] // EndsAtUtc before StartsAtUtc
    public void Create_WithEndsAtUtcNotAfterStartsAtUtc_Fails(int hoursAfterStart)
    {
        var start = Now.AddDays(10);
        var result = CreateSchedule(startsAtUtc: start, endsAtUtc: start.AddHours(hoursAfterStart));

        Assert.True(result.IsFailure);
        Assert.Equal("EventSchedule.EndsBeforeStarts", result.Error.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithCapacityLessThanOne_Fails(int capacity)
    {
        var result = CreateSchedule(capacity: capacity);

        Assert.True(result.IsFailure);
        Assert.Equal("EventSchedule.CapacityInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithNullCapacity_SucceedsAsUnlimited()
    {
        var result = CreateSchedule(capacity: null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Capacity);
    }

    [Fact]
    public void Create_WithRegistrationClosesAfterStart_Fails()
    {
        var start = Now.AddDays(10);
        var result = CreateSchedule(startsAtUtc: start, registrationClosesAtUtc: start.AddMinutes(1));

        Assert.True(result.IsFailure);
        Assert.Equal("EventSchedule.RegistrationClosesAfterStart", result.Error.Code);
    }

    [Fact]
    public void Create_WithRegistrationClosesAtOrBeforeStart_Succeeds()
    {
        var start = Now.AddDays(10);
        var result = CreateSchedule(startsAtUtc: start, registrationClosesAtUtc: start);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithMinAgeGreaterThanMaxAge_Fails()
    {
        var result = CreateSchedule(minAge: 18, maxAge: 12);

        Assert.True(result.IsFailure);
        Assert.Equal("EventSchedule.MinAgeGreaterThanMaxAge", result.Error.Code);
    }

    [Fact]
    public void Create_WithNegativeAge_Fails()
    {
        var result = CreateSchedule(minAge: -1);

        Assert.True(result.IsFailure);
        Assert.Equal("EventSchedule.AgeInvalid", result.Error.Code);
    }

    [Theory]
    [InlineData(EventFormat.Online)]
    [InlineData(EventFormat.Hybrid)]
    public void Create_OnlineOrHybridWithoutOnlineLink_Fails(EventFormat format)
    {
        var result = CreateSchedule(format: format, onlineLink: null);

        Assert.True(result.IsFailure);
        Assert.Equal("EventSchedule.OnlineLinkRequired", result.Error.Code);
    }

    [Theory]
    [InlineData(EventFormat.Online)]
    [InlineData(EventFormat.Hybrid)]
    public void Create_OnlineOrHybridWithHttpsOnlineLink_Succeeds(EventFormat format)
    {
        var result = CreateSchedule(format: format, onlineLink: "https://meet.example.com/etkinlik");

        Assert.True(result.IsSuccess);
        Assert.Equal("https://meet.example.com/etkinlik", result.Value.OnlineLink);
    }

    [Fact]
    public void Create_InPersonWithoutOnlineLink_Succeeds()
    {
        var result = CreateSchedule(format: EventFormat.InPerson, onlineLink: null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.OnlineLink);
    }

    [Fact]
    public void Create_WithNonHttpsOnlineLink_Fails()
    {
        var result = CreateSchedule(format: EventFormat.Online, onlineLink: "http://meet.example.com/etkinlik");

        Assert.True(result.IsFailure);
        Assert.Equal("EventSchedule.OnlineLinkInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithRelativeOnlineLink_Fails()
    {
        var result = CreateSchedule(format: EventFormat.Online, onlineLink: "/etkinlik");

        Assert.True(result.IsFailure);
        Assert.Equal("EventSchedule.OnlineLinkInvalid", result.Error.Code);
    }

    [Fact]
    public void UpdateSchedule_BeforeEventStarted_WithValidChanges_Succeeds()
    {
        var schedule = CreateSchedule(startsAtUtc: Now.AddDays(10)).Value;

        var result = schedule.UpdateSchedule(
            Now.AddDays(20), Now.AddDays(20).AddHours(3), EventFormat.Online, "https://meet.example.com/yeni", 50, true, null, null,
            null, null, false, false, Now, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now.AddDays(20), schedule.StartsAtUtc);
        Assert.Equal(50, schedule.Capacity);
        Assert.Equal(EventFormat.Online, schedule.Format);
    }

    [Fact]
    public void UpdateSchedule_AfterEventStarted_WithChangedCapacity_Fails()
    {
        var startsAtUtc = Now.AddDays(-1);
        var schedule = CreateSchedule(startsAtUtc: startsAtUtc, endsAtUtc: startsAtUtc.AddHours(2)).Value;

        var result = schedule.UpdateSchedule(
            startsAtUtc, startsAtUtc.AddHours(2), EventFormat.InPerson, null, 999, true, null, null, null, null, true, true, Now, UserId,
            Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Event.CannotModifyScheduleAfterStart", result.Error.Code);
    }

    [Fact]
    public void UpdateSchedule_AfterEventStarted_WithUnchangedValues_Succeeds()
    {
        var startsAtUtc = Now.AddDays(-1);
        var endsAtUtc = startsAtUtc.AddHours(2);
        var schedule = CreateSchedule(startsAtUtc: startsAtUtc, endsAtUtc: endsAtUtc, capacity: 10).Value;

        var result = schedule.UpdateSchedule(
            startsAtUtc, endsAtUtc, EventFormat.InPerson, null, 10, true, null, null, null, null, true, true, Now, UserId, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void UpdateSchedule_AfterEventStarted_TextsCanStillBeUpdated()
    {
        var startsAtUtc = Now.AddDays(-1);
        var endsAtUtc = startsAtUtc.AddHours(2);
        var schedule = CreateSchedule(startsAtUtc: startsAtUtc, endsAtUtc: endsAtUtc).Value;

        var result = schedule.SetTranslation(Tr, "Yeni Salon", "Yeni Adres", "Ücretsiz", "Yeni Eğitmen", "<p>Yeni</p>", "Not", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Yeni Salon", schedule.Translations[0].VenueName);
    }

    [Fact]
    public void SetTranslation_AddsNewLanguageTranslation()
    {
        var schedule = CreateSchedule().Value;

        var result = schedule.SetTranslation(En, "Venue Hall", "Sample Address", "Free", "Instructor", "<p>Program</p>", "Accessible", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, schedule.Translations.Count);
        Assert.Contains(schedule.Translations, t => t.LanguageCode == En && t.VenueName == "Venue Hall");
    }

    [Fact]
    public void Cancel_SetsIsCancelledAndReason()
    {
        var schedule = CreateSchedule().Value;

        var result = schedule.Cancel("Hava koşulları nedeniyle iptal edildi.", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.True(schedule.IsCancelled);
        Assert.Equal(Now, schedule.CancelledAtUtc);
        Assert.Equal("Hava koşulları nedeniyle iptal edildi.", schedule.CancellationReason);
    }

    [Fact]
    public void Cancel_WithReasonTooLong_Fails()
    {
        var schedule = CreateSchedule().Value;

        var result = schedule.Cancel(new string('a', 501), UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("EventSchedule.CancellationReasonTooLong", result.Error.Code);
    }

    [Fact]
    public void Reactivate_AfterCancel_ClearsCancellationState()
    {
        var schedule = CreateSchedule().Value;
        schedule.Cancel("iptal", UserId, Now);

        var result = schedule.Reactivate(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.False(schedule.IsCancelled);
        Assert.Null(schedule.CancelledAtUtc);
        Assert.Null(schedule.CancellationReason);
    }
}
