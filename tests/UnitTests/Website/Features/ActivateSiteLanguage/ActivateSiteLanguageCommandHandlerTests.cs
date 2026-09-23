using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.ActivateSiteLanguage;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.ActivateSiteLanguage;

public class ActivateSiteLanguageCommandHandlerTests
{
    private readonly FakeSiteLanguageRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ActivateSiteLanguageCommandHandler CreateHandler() =>
        new(_repository, new FakeCurrentUserContext(Guid.NewGuid()), _unitOfWork);

    [Fact]
    public async Task Handle_WithInactiveLanguage_Activates()
    {
        var language = SiteLanguage.Create(LanguageCode.Create("en").Value, "English", 2, Guid.NewGuid(), DateTime.UtcNow);
        language.Deactivate(Guid.NewGuid(), DateTime.UtcNow);
        _repository.Seed(language);

        var result = await CreateHandler().Handle(new ActivateSiteLanguageCommand(language.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(language.IsActive);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new ActivateSiteLanguageCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
