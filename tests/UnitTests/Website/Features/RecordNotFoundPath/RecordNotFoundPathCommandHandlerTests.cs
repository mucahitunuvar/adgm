using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.RecordNotFoundPath;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.RecordNotFoundPath;

public class RecordNotFoundPathCommandHandlerTests
{
    private readonly FakeNotFoundLogRepository _notFoundLogRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private RecordNotFoundPathCommandHandler CreateHandler() => new(_notFoundLogRepository, _unitOfWork);

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
}
