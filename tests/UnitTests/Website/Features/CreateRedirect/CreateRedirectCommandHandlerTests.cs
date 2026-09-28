using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.CreateRedirect;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.CreateRedirect;

public class CreateRedirectCommandHandlerTests
{
    private readonly FakeRedirectRepository _redirectRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateRedirectCommandHandler CreateHandler() =>
        new(_redirectRepository, _contentItemRepository, new FakeCurrentUserContext(Guid.NewGuid()), _unitOfWork);

    [Fact]
    public async Task Handle_WithPathTarget_CreatesRedirect()
    {
        var command = new CreateRedirectCommand("tr", "eski-yol", "Path", null, "yeni-yol", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("eski-yol", result.Value.FromPath);
        Assert.Single(_redirectRepository.Redirects);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithContentItemTargetThatDoesNotExist_ReturnsNotFound()
    {
        var command = new CreateRedirectCommand("tr", "eski-yol", "ContentItem", Guid.NewGuid(), null, "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.TargetContentItemNotFound", result.Error.Code);
        Assert.Empty(_redirectRepository.Redirects);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenFromPathCollidesWithLiveContentItem_ReturnsConflict()
    {
        _contentItemRepository.FullPathExistsResult = true;
        var command = new CreateRedirectCommand("tr", "canli-icerik", "Path", null, "hedef", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.FromPathCollidesWithContentItem", result.Error.Code);
        Assert.Empty(_redirectRepository.Redirects);
    }

    [Fact]
    public async Task Handle_WhenFromPathAlreadyHasARedirect_ReturnsConflict()
    {
        var existing = Redirect.Create(
            LanguageCode.Create("tr").Value, "eski-yol", RedirectTargetKind.Path, null, "baska-yol",
            RedirectStatusCode.MovedPermanently, Guid.NewGuid(), DateTime.UtcNow).Value;
        _redirectRepository.Seed(existing);
        var command = new CreateRedirectCommand("tr", "eski-yol", "Path", null, "yeni-yol", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.FromPathAlreadyExists", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenTargetPathIsItselfAnotherRedirectsFromPath_ReturnsConflictToPreventChain()
    {
        var existing = Redirect.Create(
            LanguageCode.Create("tr").Value, "orta-yol", RedirectTargetKind.Path, null, "son-yol",
            RedirectStatusCode.MovedPermanently, Guid.NewGuid(), DateTime.UtcNow).Value;
        _redirectRepository.Seed(existing);
        var command = new CreateRedirectCommand("tr", "ilk-yol", "Path", null, "orta-yol", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.TargetWouldCreateChain", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithInvalidTargetKind_ReturnsValidationFailure()
    {
        var command = new CreateRedirectCommand("tr", "eski-yol", "NotARealKind", null, "yeni-yol", "MovedPermanently");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.InvalidTargetKind", result.Error.Code);
    }
}
