using GenclikMerkezi.Modules.Website.Application.Events;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.UnitTests.Website.Application.Events;

// ADR-024 §11.2 (Faz 4 Görev 3) - "EventSchedule.RowVersion çakışmasında en fazla 3 kez yeniden
// denenir, sonra 409". FakeUnitOfWork.FailNextSaveChangesWith/OnSaveChangesFailure simulate the real
// DbUpdateConcurrencyException SaveChangesAsync would throw without needing a real database.
public class EventCapacityConcurrencyRetryExecutorTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid ContentItemId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private static EventSchedule CreateSchedule() =>
        EventSchedule.Create(
            ContentItemId, Now.AddDays(10), Now.AddDays(10).AddHours(2), EventFormat.InPerson, null, 10, registrationEnabled: true,
            registrationOpensAtUtc: null, registrationClosesAtUtc: null, minAge: null, maxAge: null, autoConfirm: true,
            waitlistEnabled: true, Tr, "Salon", "Adres", "Ücretsiz", "Eğitmen", "<p/>", "Not", UserId, Now).Value;

    [Fact]
    public async Task ExecuteAsync_WhenMutateFails_ReturnsFailureWithoutCallingSaveChanges()
    {
        var unitOfWork = new FakeUnitOfWork();
        var executor = new EventCapacityConcurrencyRetryExecutor(new FakeEventScheduleRepository(), unitOfWork);
        var schedule = CreateSchedule();

        var result = await executor.ExecuteAsync(
            schedule, () => Result.Failure(Error.Conflict("Event.CapacityFull", "full")), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Event.CapacityFull", result.Error.Code);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSaveChangesSucceedsFirstTry_ReturnsSuccess()
    {
        var unitOfWork = new FakeUnitOfWork();
        var executor = new EventCapacityConcurrencyRetryExecutor(new FakeEventScheduleRepository(), unitOfWork);
        var schedule = CreateSchedule();

        var result = await executor.ExecuteAsync(schedule, () => Result.Success(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_OnSingleConcurrencyConflict_ReloadsAndRetriesThenSucceeds()
    {
        var unitOfWork = new FakeUnitOfWork { FailNextSaveChangesWith = new DbUpdateConcurrencyException() };
        var executor = new EventCapacityConcurrencyRetryExecutor(new FakeEventScheduleRepository(), unitOfWork);
        var schedule = CreateSchedule();
        var mutateCallCount = 0;

        var result = await executor.ExecuteAsync(
            schedule,
            () =>
            {
                mutateCallCount++;
                return Result.Success();
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, unitOfWork.SaveChangesCallCount);
        Assert.Equal(2, mutateCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenConflictPersistsBeyondMaxAttempts_ReturnsConflictError()
    {
        var unitOfWork = new FakeUnitOfWork { FailNextSaveChangesWith = new DbUpdateConcurrencyException() };
        unitOfWork.OnSaveChangesFailure = () => unitOfWork.FailNextSaveChangesWith = new DbUpdateConcurrencyException();
        var executor = new EventCapacityConcurrencyRetryExecutor(new FakeEventScheduleRepository(), unitOfWork);
        var schedule = CreateSchedule();

        var result = await executor.ExecuteAsync(schedule, Result.Success, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Event.RegistrationConflict", result.Error.Code);
        Assert.Equal(EventCapacityConcurrencyRetryExecutor.MaxAttempts, unitOfWork.SaveChangesCallCount);
    }
}
