using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.RecordNotFoundPath;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.Features.RecordNotFoundPath;

public class RecordNotFoundPathCommandHandlerTests
{
    private readonly FakeNotFoundLogRepository _notFoundLogRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _timeProvider = new(DateTimeOffset.UtcNow);

    private RecordNotFoundPathCommandHandler CreateHandler() =>
        new(_notFoundLogRepository, _unitOfWork, _timeProvider, NullLogger<RecordNotFoundPathCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WithNewPath_CreatesLogWithHitCountOne()
    {
        var result = await CreateHandler().Handle(new RecordNotFoundPathCommand("tr", "yeni-404"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var log = Assert.Single(_notFoundLogRepository.NotFoundLogs);
        Assert.Equal("yeni-404", log.Path);
        Assert.Equal(1, log.HitCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithAlreadyKnownPath_BumpsHitCountInsteadOfCreatingANewRow()
    {
        var existing = NotFoundLog.Create(LanguageCode.Create("tr").Value, "bilinen-404", DateTime.UtcNow.AddDays(-1)).Value;
        _notFoundLogRepository.Seed(existing);

        var result = await CreateHandler().Handle(new RecordNotFoundPathCommand("tr", "bilinen-404"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var log = Assert.Single(_notFoundLogRepository.NotFoundLogs);
        Assert.Equal(2, log.HitCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenDistinctPathCapIsReached_DoesNotCreateNewPath_ButSucceeds()
    {
        for (var i = 0; i < 10_000; i++)
        {
            _notFoundLogRepository.Seed(NotFoundLog.Create(LanguageCode.Create("tr").Value, $"yol-{i}", DateTime.UtcNow).Value);
        }

        var result = await CreateHandler().Handle(new RecordNotFoundPathCommand("tr", "kapasite-asan-yeni-yol"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(10_000, _notFoundLogRepository.NotFoundLogs.Count);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenDistinctPathCapIsReached_StillBumpsAnExistingPath()
    {
        NotFoundLog? target = null;
        for (var i = 0; i < 10_000; i++)
        {
            var log = NotFoundLog.Create(LanguageCode.Create("tr").Value, $"yol-{i}", DateTime.UtcNow).Value;
            _notFoundLogRepository.Seed(log);
            target ??= log;
        }

        var result = await CreateHandler().Handle(new RecordNotFoundPathCommand("tr", target!.Path), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, target.HitCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithInvalidLanguageCode_ReturnsValidationFailure()
    {
        var result = await CreateHandler().Handle(new RecordNotFoundPathCommand("not-a-code", "yol"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Empty(_notFoundLogRepository.NotFoundLogs);
    }

    // Fix (post-Faz-1a review): a (LanguageCode, Path) race - a concurrent request's insert lands
    // between this handler's own "does it already exist" check and its insert, so this handler's
    // SaveChangesAsync fails. Simulated here by making the fake UnitOfWork's next SaveChangesAsync
    // throw and, as that throw's side effect, seeding the row a concurrent winner would have created.
    [Fact]
    public async Task Handle_WhenCreateRacesAConcurrentInsert_RetriesAsHitBumpAndSucceeds()
    {
        var languageCode = LanguageCode.Create("tr").Value;
        _unitOfWork.FailNextSaveChangesWith = new InvalidOperationException("simulated (LanguageCode, Path) unique index violation");
        _unitOfWork.OnSaveChangesFailure = () =>
            _notFoundLogRepository.Seed(NotFoundLog.Create(languageCode, "yaris-404", DateTime.UtcNow).Value);

        var result = await CreateHandler().Handle(new RecordNotFoundPathCommand("tr", "yaris-404"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var log = Assert.Single(_notFoundLogRepository.NotFoundLogs);
        Assert.Equal("yaris-404", log.Path);
        Assert.Equal(2, log.HitCount);
        Assert.Equal(2, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenCreateFailsAndRetryFindsNothing_StillReturnsSuccess_WithoutCreatingADuplicateRow()
    {
        _unitOfWork.FailNextSaveChangesWith = new InvalidOperationException("simulated failure unrelated to a concurrent insert");

        var result = await CreateHandler().Handle(new RecordNotFoundPathCommand("tr", "kaybolan-404"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_notFoundLogRepository.NotFoundLogs);
    }
}
