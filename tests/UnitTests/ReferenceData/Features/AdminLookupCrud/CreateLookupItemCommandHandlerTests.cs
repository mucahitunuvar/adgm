using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.ReferenceData.Features.AdminLookupCrud;

public class CreateLookupItemCommandHandlerTests
{
    private readonly FakeAdminLookupCrudService<Sector> _crudService = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateLookupItemCommandHandler<Sector> CreateHandler() => new(_crudService, _cacheService, _unitOfWork);

    [Fact]
    public async Task Handle_WithNewCode_AddsEntityAndSavesChanges()
    {
        var result = await CreateHandler().Handle(
            new CreateLookupItemCommand<Sector>("IT", "Bilişim", 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(_crudService.Items);
        Assert.Equal("IT", _crudService.Items.First().Code);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithDuplicateCode_ReturnsConflict_AndDoesNotSave()
    {
        _crudService.Add(Sector.Create("IT", "Bilişim", 0));

        var result = await CreateHandler().Handle(
            new CreateLookupItemCommand<Sector>("IT", "Yeni Bilişim", 1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Lookup.CodeAlreadyExists", result.Error.Code);
        Assert.Single(_crudService.Items);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
