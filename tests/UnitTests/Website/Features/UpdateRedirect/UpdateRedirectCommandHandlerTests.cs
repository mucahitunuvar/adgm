using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.UpdateRedirect;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.UpdateRedirect;

public class UpdateRedirectCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;

    private readonly FakeRedirectRepository _redirectRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private UpdateRedirectCommandHandler CreateHandler() =>
        new(_redirectRepository, _contentItemRepository, new FakeCurrentUserContext(Guid.NewGuid()), _cacheService, _unitOfWork);

    private Redirect SeedManualRedirect(string fromPath = "eski-yol", string targetPath = "hedef-1")
    {
        var redirect = Redirect.Create(
            Tr, fromPath, RedirectTargetKind.Path, null, targetPath, RedirectStatusCode.MovedPermanently, Guid.NewGuid(), DateTime.UtcNow).Value;
        _redirectRepository.Seed(redirect);
        return redirect;
    }

    [Fact]
    public async Task Handle_WithNonExistentRedirect_ReturnsNotFound()
    {
        var command = new UpdateRedirectCommand(Guid.NewGuid(), "Path", null, "hedef-2", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_OnManualRedirect_UpdatesTarget()
    {
        var redirect = SeedManualRedirect();
        var command = new UpdateRedirectCommand(redirect.Id, "Path", null, "hedef-2", "Found");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("hedef-2", redirect.TargetPath);
        Assert.Equal(RedirectStatusCode.Found, redirect.StatusCode);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_OnAutomaticRedirect_Fails()
    {
        var automatic = Redirect.CreateAutomatic(Tr, "otomatik-yol", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow).Value;
        _redirectRepository.Seed(automatic);
        var command = new UpdateRedirectCommand(automatic.Id, "Path", null, "hedef", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.CannotEditAutomaticRedirect", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenNewTargetPathIsItselfAnotherRedirectsFromPath_ReturnsConflict()
    {
        var chained = SeedManualRedirect("baska-kaynak", "baska-hedef");
        var redirect = SeedManualRedirect();
        var command = new UpdateRedirectCommand(redirect.Id, "Path", null, "baska-kaynak", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.TargetWouldCreateChain", result.Error.Code);
        Assert.NotEqual("baska-kaynak", redirect.TargetPath);
    }

    // GÖREV 1: the chain-check pre-check used to normalize its own lookup candidate with a plain
    // Trim('/') (leaving a backslash untouched), separately from redirect.Update()'s own validation.
    // Now it reuses Redirect.NormalizeTargetPath and skips the lookup on failure, so the real
    // "TargetPathInvalid" error from Update() below is what actually surfaces here - not a false
    // "chain" conflict and not a silent pass-through.
    [Fact]
    public async Task Handle_WithBackslashInTargetPath_ReturnsTargetPathInvalid()
    {
        var redirect = SeedManualRedirect();
        var command = new UpdateRedirectCommand(redirect.Id, "Path", null, "\\evil.example.com", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.TargetPathInvalid", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithNewTargetPathEqualToOwnFromPath_Fails()
    {
        var redirect = SeedManualRedirect();
        var command = new UpdateRedirectCommand(redirect.Id, "Path", null, redirect.FromPath, "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.TargetCannotBeFromPath", result.Error.Code);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("Path,ContentItem")] // 0|1 = 1 = ContentItem - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedTargetKind_Fails(string targetKind)
    {
        var redirect = SeedManualRedirect();
        var command = new UpdateRedirectCommand(redirect.Id, targetKind, null, "hedef-2", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.InvalidTargetKind", result.Error.Code);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("MovedPermanently,Found")] // 301|302 = 303, not a defined member - still correctly rejected.
    public async Task Handle_WithNumericOrCombinedStatusCode_Fails(string statusCode)
    {
        var redirect = SeedManualRedirect();
        var command = new UpdateRedirectCommand(redirect.Id, "Path", null, "hedef-2", statusCode);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.InvalidStatusCode", result.Error.Code);
    }
}
