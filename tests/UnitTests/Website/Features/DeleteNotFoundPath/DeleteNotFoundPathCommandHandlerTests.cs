using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.DeleteNotFoundPath;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.DeleteNotFoundPath;

public class DeleteNotFoundPathCommandHandlerTests
{
    private readonly FakeNotFoundLogRepository _notFoundLogRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private DeleteNotFoundPathCommandHandler CreateHandler() => new(_notFoundLogRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithNonExistentLog_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new DeleteNotFoundPathCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFoundLog.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithExistingLog_RemovesIt()
    {
        var log = NotFoundLog.Create(LanguageCode.Create("tr").Value, "eski-sayfa", DateTime.UtcNow).Value;
        _notFoundLogRepository.Seed(log);

        var result = await CreateHandler().Handle(new DeleteNotFoundPathCommand(log.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_notFoundLogRepository.NotFoundLogs);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }
}
