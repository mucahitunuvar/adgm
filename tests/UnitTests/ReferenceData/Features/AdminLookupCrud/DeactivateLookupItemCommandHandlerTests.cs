using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.ReferenceData.Features.AdminLookupCrud;

public class DeactivateLookupItemCommandHandlerTests
{
    private readonly FakeAdminLookupCrudService<Sector> _crudService = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));
    private readonly FakeUnitOfWork _unitOfWork = new();

    private DeactivateLookupItemCommandHandler<Sector> CreateHandler() => new(_crudService, _cacheService, _unitOfWork);

    [Fact]
    public async Task Handle_WithExistingId_DeactivatesButDoesNotRemoveTheRow()
    {
        var sector = Sector.Create("IT", "Bilişim", 0);
        _crudService.Add(sector);

        var result = await CreateHandler().Handle(new DeactivateLookupItemCommand<Sector>(sector.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(sector.IsActive);
        // Referential integrity: the row still exists (soft-delete, not a real delete) - anything
        // that already referenced this id by value can still resolve it.
        Assert.Single(_crudService.Items);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new DeactivateLookupItemCommand<Sector>(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Lookup.NotFound", result.Error.Code);
    }
}
