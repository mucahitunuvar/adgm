using GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using TaxOffice = GenclikMerkezi.Modules.ReferenceData.Domain.TaxOffice;

namespace GenclikMerkezi.UnitTests.ReferenceData.Features.AdminTaxOffice;

public class UpdateTaxOfficeCommandHandlerTests
{
    private readonly FakeAdminLookupCrudService<TaxOffice> _crudService = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateTaxOfficeCommandHandler CreateHandler() => new(_crudService, _cache, _unitOfWork);

    [Fact]
    public async Task Handle_WithExistingId_UpdatesDisplayNameAndSortOrder_ButNotProvinceId()
    {
        var provinceId = Guid.NewGuid();
        var taxOffice = TaxOffice.Create("1250", "Eski İsim", 0, provinceId);
        _crudService.Add(taxOffice);

        var result = await CreateHandler().Handle(
            new UpdateTaxOfficeCommand(taxOffice.Id, "Yeni İsim", 3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Yeni İsim", taxOffice.DisplayName);
        Assert.Equal(3, taxOffice.SortOrder);
        Assert.Equal(provinceId, taxOffice.ProvinceId);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new UpdateTaxOfficeCommand(Guid.NewGuid(), "X", 0), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Lookup.NotFound", result.Error.Code);
    }
}
