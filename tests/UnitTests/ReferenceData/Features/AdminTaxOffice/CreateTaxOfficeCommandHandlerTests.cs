using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using TaxOffice = GenclikMerkezi.Modules.ReferenceData.Domain.TaxOffice;

namespace GenclikMerkezi.UnitTests.ReferenceData.Features.AdminTaxOffice;

public class CreateTaxOfficeCommandHandlerTests
{
    private readonly FakeAdminLookupCrudService<TaxOffice> _crudService = new();
    private readonly FakeReferenceDataLookupReader _lookupReader = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateTaxOfficeCommandHandler CreateHandler() => new(_crudService, _lookupReader, _cache, _unitOfWork);

    [Fact]
    public async Task Handle_WithActiveProvinceAndNewCode_CreatesTaxOffice()
    {
        var provinceId = Guid.NewGuid();
        _lookupReader.SeedActive(ReferenceDataLookupType.Province, provinceId);

        var result = await CreateHandler().Handle(
            new CreateTaxOfficeCommand("1250", "Adana İhtisas Vergi Dairesi", 0, provinceId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(_crudService.Items);
        Assert.Equal(provinceId, _crudService.Items.First().ProvinceId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownProvince_ReturnsNotFound_AndDoesNotCreate()
    {
        var result = await CreateHandler().Handle(
            new CreateTaxOfficeCommand("1250", "Adana İhtisas Vergi Dairesi", 0, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("TaxOffice.ProvinceNotFound", result.Error.Code);
        Assert.Empty(_crudService.Items);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithDuplicateCode_ReturnsConflict()
    {
        var provinceId = Guid.NewGuid();
        _lookupReader.SeedActive(ReferenceDataLookupType.Province, provinceId);
        _crudService.Add(TaxOffice.Create("1250", "Mevcut Vergi Dairesi", 0, provinceId));

        var result = await CreateHandler().Handle(
            new CreateTaxOfficeCommand("1250", "Yeni Vergi Dairesi", 1, provinceId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Lookup.CodeAlreadyExists", result.Error.Code);
    }
}
