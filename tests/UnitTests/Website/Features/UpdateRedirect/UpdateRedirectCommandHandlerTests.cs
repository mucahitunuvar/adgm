using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.UpdateRedirect;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.UpdateRedirect;

public class UpdateRedirectCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;

    private readonly FakeRedirectRepository _redirectRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateRedirectCommandHandler CreateHandler() =>
        new(_redirectRepository, _contentItemRepository, new FakeCurrentUserContext(Guid.NewGuid()), _unitOfWork);

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

    [Fact]
    public async Task Handle_WithNewTargetPathEqualToOwnFromPath_Fails()
    {
        var redirect = SeedManualRedirect();
        var command = new UpdateRedirectCommand(redirect.Id, "Path", null, redirect.FromPath, "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.TargetCannotBeFromPath", result.Error.Code);
    }
}
