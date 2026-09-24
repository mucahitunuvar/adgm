using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteLanguage;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.UpdateSiteLanguage;

public class UpdateSiteLanguageCommandHandlerTests
{
    private readonly FakeSiteLanguageRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private UpdateSiteLanguageCommandHandler CreateHandler() =>
        new(_repository, new FakeCurrentUserContext(Guid.NewGuid()), _cacheService, _unitOfWork);

    [Fact]
    public async Task Handle_WithExistingLanguage_UpdatesNameAndSortOrder()
    {
        var language = SiteLanguage.Create(LanguageCode.Create("tr").Value, "Türkçe", 1, Guid.NewGuid(), DateTime.UtcNow);
        _repository.Seed(language);

        var result = await CreateHandler().Handle(
            new UpdateSiteLanguageCommand(language.Id, "Türkçe (Güncel)", 9), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Türkçe (Güncel)", language.Name);
        Assert.Equal(9, language.SortOrder);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new UpdateSiteLanguageCommand(Guid.NewGuid(), "Adı", 1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
