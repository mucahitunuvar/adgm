using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.SetDefaultSiteLanguage;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.SetDefaultSiteLanguage;

public class SetDefaultSiteLanguageCommandHandlerTests
{
    private readonly FakeSiteLanguageRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private SetDefaultSiteLanguageCommandHandler CreateHandler() =>
        new(_repository, new FakeCurrentUserContext(Guid.NewGuid()), _unitOfWork);

    [Fact]
    public async Task Handle_WithActiveNonDefaultLanguage_SwapsDefaultAtomically()
    {
        var currentDefault = SiteLanguage.Create(LanguageCode.Create("tr").Value, "Türkçe", 1, Guid.NewGuid(), DateTime.UtcNow);
        currentDefault.MarkAsDefault(Guid.NewGuid(), DateTime.UtcNow);
        var candidate = SiteLanguage.Create(LanguageCode.Create("en").Value, "English", 2, Guid.NewGuid(), DateTime.UtcNow);
        _repository.Seed(currentDefault);
        _repository.Seed(candidate);

        var result = await CreateHandler().Handle(new SetDefaultSiteLanguageCommand(candidate.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(candidate.IsDefault);
        Assert.False(currentDefault.IsDefault);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithInactiveLanguage_ReturnsConflict_AndDoesNotTouchExistingDefault()
    {
        var currentDefault = SiteLanguage.Create(LanguageCode.Create("tr").Value, "Türkçe", 1, Guid.NewGuid(), DateTime.UtcNow);
        currentDefault.MarkAsDefault(Guid.NewGuid(), DateTime.UtcNow);
        var candidate = SiteLanguage.Create(LanguageCode.Create("en").Value, "English", 2, Guid.NewGuid(), DateTime.UtcNow);
        candidate.Deactivate(Guid.NewGuid(), DateTime.UtcNow);
        _repository.Seed(currentDefault);
        _repository.Seed(candidate);

        var result = await CreateHandler().Handle(new SetDefaultSiteLanguageCommand(candidate.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("SiteLanguage.InactiveCannotBeDefault", result.Error.Code);
        Assert.False(candidate.IsDefault);
        Assert.True(currentDefault.IsDefault);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenAlreadyDefault_IsIdempotent()
    {
        var language = SiteLanguage.Create(LanguageCode.Create("tr").Value, "Türkçe", 1, Guid.NewGuid(), DateTime.UtcNow);
        language.MarkAsDefault(Guid.NewGuid(), DateTime.UtcNow);
        _repository.Seed(language);

        var result = await CreateHandler().Handle(new SetDefaultSiteLanguageCommand(language.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new SetDefaultSiteLanguageCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
