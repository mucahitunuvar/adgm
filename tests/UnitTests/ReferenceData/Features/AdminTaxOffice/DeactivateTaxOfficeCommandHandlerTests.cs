using GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using TaxOffice = GenclikMerkezi.Modules.ReferenceData.Domain.TaxOffice;

namespace GenclikMerkezi.UnitTests.ReferenceData.Features.AdminTaxOffice;

public class DeactivateTaxOfficeCommandHandlerTests
{
    private readonly FakeAdminLookupCrudService<TaxOffice> _crudService = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly FakeUnitOfWork _unitOfWork = new();

    private DeactivateTaxOfficeCommandHandler CreateHandler() => new(_crudService, _cache, _unitOfWork);

    [Fact]
    public async Task Handle_WithExistingId_DeactivatesButPreservesProvinceIdAndRow()
    {
        var provinceId = Guid.NewGuid();
        var taxOffice = TaxOffice.Create("1250", "Adana İhtisas Vergi Dairesi", 0, provinceId);
        _crudService.Add(taxOffice);

        var result = await CreateHandler().Handle(new DeactivateTaxOfficeCommand(taxOffice.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(taxOffice.IsActive);
        // Referential integrity: the row (and its ProvinceId) survives the soft-delete.
        Assert.Single(_crudService.Items);
        Assert.Equal(provinceId, taxOffice.ProvinceId);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new DeactivateTaxOfficeCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Lookup.NotFound", result.Error.Code);
    }
}
