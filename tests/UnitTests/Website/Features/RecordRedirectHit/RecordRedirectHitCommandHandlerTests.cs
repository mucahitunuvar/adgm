using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.RecordRedirectHit;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.Features.RecordRedirectHit;

public class RecordRedirectHitCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeRedirectRepository _redirectRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _timeProvider = new(Now);

    private RecordRedirectHitCommandHandler CreateHandler() =>
        new(_redirectRepository, _unitOfWork, _timeProvider, NullLogger<RecordRedirectHitCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WithExistingRedirect_IncrementsHitCountAndSetsLastHitAtUtc()
    {
        var redirect = Redirect.CreateAutomatic(Tr, "eski-yol", Guid.NewGuid(), UserId, Now.AddDays(-1)).Value;
        _redirectRepository.Seed(redirect);

        var result = await CreateHandler().Handle(new RecordRedirectHitCommand(redirect.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, redirect.HitCount);
        Assert.Equal(Now, redirect.LastHitAtUtc);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_CalledTwice_AccumulatesHitCount()
    {
        var redirect = Redirect.CreateAutomatic(Tr, "eski-yol", Guid.NewGuid(), UserId, Now.AddDays(-1)).Value;
        _redirectRepository.Seed(redirect);

        await CreateHandler().Handle(new RecordRedirectHitCommand(redirect.Id), CancellationToken.None);
        await CreateHandler().Handle(new RecordRedirectHitCommand(redirect.Id), CancellationToken.None);

        Assert.Equal(2, redirect.HitCount);
    }

    [Fact]
    public async Task Handle_WithUnknownRedirectId_ReturnsSuccessWithoutSaving()
    {
        var result = await CreateHandler().Handle(new RecordRedirectHitCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_StillReturnsSuccess()
    {
        var redirect = Redirect.CreateAutomatic(Tr, "eski-yol", Guid.NewGuid(), UserId, Now.AddDays(-1)).Value;
        _redirectRepository.Seed(redirect);
        _unitOfWork.FailNextSaveChangesWith = new InvalidOperationException("simulated concurrency conflict");

        var result = await CreateHandler().Handle(new RecordRedirectHitCommand(redirect.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
