using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.DiscardContentLayoutDraft;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.DiscardContentLayoutDraft;

public class DiscardContentLayoutDraftCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakePageLayoutRepository _pageLayoutRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private DiscardContentLayoutDraftCommandHandler CreateHandler() =>
        new(_pageLayoutRepository, new FakeCurrentUserContext(UserId), new FakeTimeProvider(Now), _cacheService, _unitOfWork);

    private static LayoutBlock Block(string typeKey) =>
        LayoutBlock.Create(typeKey, 1, true, "{}", [LayoutBlockTranslation.Create(Tr, "{\"body\":\"<p/>\"}")]).Value;

    [Fact]
    public async Task Handle_UnknownLayout_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new DiscardContentLayoutDraftCommand(Guid.NewGuid(), []), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithMatchingRowVersion_ResetsDraftToPublished()
    {
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        layout.ReplaceDraftBlocks([Block("rich-text")], UserId, Now);
        layout.Publish(UserId, Now);
        layout.ReplaceDraftBlocks([Block("rich-text"), Block("rich-text")], UserId, Now);
        _pageLayoutRepository.Seed(layout);

        var result = await CreateHandler().Handle(
            new DiscardContentLayoutDraftCommand(layout.ContentItemId!.Value, layout.RowVersion), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(layout.DraftBlocks);
        Assert.False(layout.HasUnpublishedChanges);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithStaleRowVersion_ReturnsConcurrencyConflict()
    {
        var layout = PageLayout.CreateForContent(Guid.NewGuid(), UserId, Now).Value;
        layout.ReplaceDraftBlocks([Block("rich-text")], UserId, Now);
        _pageLayoutRepository.Seed(layout);
        var staleRowVersion = layout.RowVersion;
        layout.ReplaceDraftBlocks([], UserId, Now.AddMinutes(1));

        var result = await CreateHandler().Handle(
            new DiscardContentLayoutDraftCommand(layout.ContentItemId!.Value, staleRowVersion), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PageLayout.ConcurrencyConflict", result.Error.Code);
    }
}
