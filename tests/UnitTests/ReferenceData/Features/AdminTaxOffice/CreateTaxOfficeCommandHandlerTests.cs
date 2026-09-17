using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TaxOffice = GenclikMerkezi.Modules.ReferenceData.Domain.TaxOffice;

namespace GenclikMerkezi.UnitTests.ReferenceData.Features.AdminTaxOffice;

public class CreateTaxOfficeCommandHandlerTests
{
    private readonly FakeAdminLookupCrudService<TaxOffice> _crudService = new();
    private readonly FakeReferenceDataLookupReader _lookupReader = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateTaxOfficeCommandHandler CreateHandler() => new(_crudService, _lookupReader, _cacheService, _unitOfWork);

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
