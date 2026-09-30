using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.ConvertNotFoundPathToRedirect;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.ConvertNotFoundPathToRedirect;

public class ConvertNotFoundPathToRedirectCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;

    private readonly FakeNotFoundLogRepository _notFoundLogRepository = new();
    private readonly FakeRedirectRepository _redirectRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private ConvertNotFoundPathToRedirectCommandHandler CreateHandler() =>
        new(_notFoundLogRepository, _redirectRepository, _contentItemRepository, new FakeCurrentUserContext(Guid.NewGuid()), _cacheService,
            _unitOfWork);

    private NotFoundLog SeedNotFoundLog(string path = "cok-tiklanan-404")
    {
        var log = NotFoundLog.Create(Tr, path, DateTime.UtcNow).Value;
        _notFoundLogRepository.Seed(log);
        return log;
    }

    [Fact]
    public async Task Handle_WithNonExistentNotFoundLog_ReturnsNotFound()
    {
        var command = new ConvertNotFoundPathToRedirectCommand(Guid.NewGuid(), "Path", null, "hedef", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFoundLog.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithValidTarget_CreatesRedirectAndRemovesNotFoundLogInOneTransaction()
    {
        var log = SeedNotFoundLog();
        var command = new ConvertNotFoundPathToRedirectCommand(log.Id, "Path", null, "yeni-hedef", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(log.Path, result.Value.FromPath);
        Assert.Single(_redirectRepository.Redirects);
        Assert.Empty(_notFoundLogRepository.NotFoundLogs);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenCollisionCheckFails_LeavesBothNotFoundLogAndRedirectsUntouched()
    {
        _contentItemRepository.FullPathExistsResult = true;
        var log = SeedNotFoundLog();
        var command = new ConvertNotFoundPathToRedirectCommand(log.Id, "Path", null, "hedef", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.FromPathCollidesWithContentItem", result.Error.Code);
        Assert.Empty(_redirectRepository.Redirects);
        Assert.Single(_notFoundLogRepository.NotFoundLogs);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithContentItemTargetThatDoesNotExist_ReturnsNotFound()
    {
        var log = SeedNotFoundLog();
        var command = new ConvertNotFoundPathToRedirectCommand(log.Id, "ContentItem", Guid.NewGuid(), null, "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.TargetContentItemNotFound", result.Error.Code);
        Assert.Single(_notFoundLogRepository.NotFoundLogs);
    }
}
