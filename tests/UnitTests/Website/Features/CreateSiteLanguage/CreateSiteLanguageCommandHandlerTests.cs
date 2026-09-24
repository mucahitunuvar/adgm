using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.CreateSiteLanguage;

public class CreateSiteLanguageCommandHandlerTests
{
    private readonly FakeSiteLanguageRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private CreateSiteLanguageCommandHandler CreateHandler() =>
        new(_repository, new FakeCurrentUserContext(Guid.NewGuid()), _cacheService, _unitOfWork);

    [Fact]
    public async Task Handle_WithNewCode_CreatesInactiveDefaultFalseLanguage()
    {
        var result = await CreateHandler().Handle(new CreateSiteLanguageCommand("EN", "English", 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("en", result.Value.Code);
        Assert.False(result.Value.IsDefault);
        Assert.True(result.Value.IsActive);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Single(_repository.SiteLanguages);
    }

    [Fact]
    public async Task Handle_WithInvalidCode_ReturnsValidationFailure()
    {
        var result = await CreateHandler().Handle(new CreateSiteLanguageCommand("123", "Invalid", 1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Empty(_repository.SiteLanguages);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithAlreadyExistingCode_ReturnsConflict()
    {
        _repository.Seed(SiteLanguage.Create(LanguageCode.Create("tr").Value, "Türkçe", 1, Guid.NewGuid(), DateTime.UtcNow));

        var result = await CreateHandler().Handle(new CreateSiteLanguageCommand("tr", "Türkçe 2", 2), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("SiteLanguage.CodeAlreadyExists", result.Error.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Single(_repository.SiteLanguages);
    }
}
