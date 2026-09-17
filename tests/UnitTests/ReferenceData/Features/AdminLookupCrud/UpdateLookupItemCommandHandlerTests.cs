using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;
using Microsoft.Extensions.Caching.Memory;

namespace GenclikMerkezi.UnitTests.ReferenceData.Features.AdminLookupCrud;

public class UpdateLookupItemCommandHandlerTests
{
    private readonly FakeAdminLookupCrudService<Sector> _crudService = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateLookupItemCommandHandler<Sector> CreateHandler() => new(_crudService, _cache, _unitOfWork);

    [Fact]
    public async Task Handle_WithExistingId_UpdatesDisplayNameAndSortOrder()
    {
        var sector = Sector.Create("IT", "Bilişim", 0);
        _crudService.Add(sector);

        var result = await CreateHandler().Handle(
            new UpdateLookupItemCommand<Sector>(sector.Id, "Bilişim Teknolojileri", 5), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Bilişim Teknolojileri", sector.DisplayName);
        Assert.Equal(5, sector.SortOrder);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new UpdateLookupItemCommand<Sector>(Guid.NewGuid(), "X", 0), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Lookup.NotFound", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
