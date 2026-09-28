using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.DeleteRedirect;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.DeleteRedirect;

public class DeleteRedirectCommandHandlerTests
{
    private readonly FakeRedirectRepository _redirectRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private DeleteRedirectCommandHandler CreateHandler() => new(_redirectRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithNonExistentRedirect_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new DeleteRedirectCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Redirect.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithManualRedirect_RemovesIt()
    {
        var redirect = Redirect.Create(
            LanguageCode.Create("tr").Value, "eski-yol", RedirectTargetKind.Path, null, "hedef",
            RedirectStatusCode.MovedPermanently, Guid.NewGuid(), DateTime.UtcNow).Value;
        _redirectRepository.Seed(redirect);

        var result = await CreateHandler().Handle(new DeleteRedirectCommand(redirect.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_redirectRepository.Redirects);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithAutomaticRedirect_RemovesIt()
    {
        var redirect = Redirect.CreateAutomatic(LanguageCode.Create("tr").Value, "otomatik-yol", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow).Value;
        _redirectRepository.Seed(redirect);

        var result = await CreateHandler().Handle(new DeleteRedirectCommand(redirect.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_redirectRepository.Redirects);
    }
}
