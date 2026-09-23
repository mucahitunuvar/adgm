using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.DeactivateSiteLanguage;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.DeactivateSiteLanguage;

public class DeactivateSiteLanguageCommandHandlerTests
{
    private readonly FakeSiteLanguageRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private DeactivateSiteLanguageCommandHandler CreateHandler() =>
        new(_repository, new FakeCurrentUserContext(Guid.NewGuid()), _unitOfWork);

    [Fact]
    public async Task Handle_WithNonDefaultLanguage_Deactivates()
    {
        var language = SiteLanguage.Create(LanguageCode.Create("en").Value, "English", 2, Guid.NewGuid(), DateTime.UtcNow);
        _repository.Seed(language);

        var result = await CreateHandler().Handle(new DeactivateSiteLanguageCommand(language.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(language.IsActive);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithDefaultLanguage_ReturnsConflict()
    {
        var language = SiteLanguage.Create(LanguageCode.Create("tr").Value, "Türkçe", 1, Guid.NewGuid(), DateTime.UtcNow);
        language.MarkAsDefault(Guid.NewGuid(), DateTime.UtcNow);
        _repository.Seed(language);

        var result = await CreateHandler().Handle(new DeactivateSiteLanguageCommand(language.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("SiteLanguage.CannotDeactivateDefault", result.Error.Code);
        Assert.True(language.IsActive);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new DeactivateSiteLanguageCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
