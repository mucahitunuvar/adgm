using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.CareerAdvisor.Features.GetGeneralPool;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.Features.GetGeneralPool;

public class GetGeneralPoolQueryHandlerTests
{
    private readonly FakePersonnelNeedModuleContract _personnelNeedModuleContract = new();
    private readonly FakeCompanyModuleContract _companyModuleContract = new();
    private readonly FakeReferenceDataLookupReader _referenceDataLookupReader = new();

    private GetGeneralPoolQueryHandler CreateHandler() =>
        new(_personnelNeedModuleContract, _companyModuleContract, _referenceDataLookupReader);

    private static PersonnelNeedSummary CreateSummary(
        Guid companyId, Guid positionId, Guid departmentId, Guid provinceId,
        Guid employmentTypeId, Guid workLocationTypeId, Guid experienceLevelId) =>
        new(
            Guid.NewGuid(), companyId, employmentTypeId, workLocationTypeId, positionId, departmentId,
            3, provinceId, experienceLevelId, "Acil ihtiyaç", Guid.NewGuid(), DateTime.UtcNow);

    [Fact]
    public async Task Handle_ReturnsPagedResults_WithAllNamesResolved()
    {
        var companyId = Guid.NewGuid();
        var positionId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var provinceId = Guid.NewGuid();
        var employmentTypeId = Guid.NewGuid();
        var workLocationTypeId = Guid.NewGuid();
        var experienceLevelId = Guid.NewGuid();

        var summary = CreateSummary(
            companyId, positionId, departmentId, provinceId, employmentTypeId, workLocationTypeId, experienceLevelId);
        _personnelNeedModuleContract.SeedGeneralPool(summary);

        _companyModuleContract.Seed(new CompanySummary(companyId, "Acme A.Ş.", Guid.NewGuid()));
        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.Position, new LookupItemSummary(positionId, "POS", "Kaynakçı", true, 1));
        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.Department, new LookupItemSummary(departmentId, "DEP", "Üretim", true, 1));
        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.Province, new LookupItemSummary(provinceId, "PRV", "İstanbul", true, 1));
        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.EmploymentType, new LookupItemSummary(employmentTypeId, "EMP", "Tam Zamanlı", true, 1));
        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.WorkLocationType, new LookupItemSummary(workLocationTypeId, "WLT", "Ofis", true, 1));
        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.ExperienceLevel, new LookupItemSummary(experienceLevelId, "EXP", "1-3 Yıl", true, 1));

        var result = await CreateHandler().Handle(new GetGeneralPoolQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(summary.Id, item.Id);
        Assert.Equal("Acme A.Ş.", item.CompanyName);
        Assert.Equal("Kaynakçı", item.PositionName);
        Assert.Equal("Üretim", item.DepartmentName);
        Assert.Equal("İstanbul", item.ProvinceName);
        Assert.Equal("Tam Zamanlı", item.EmploymentTypeName);
        Assert.Equal("Ofis", item.WorkLocationTypeName);
        Assert.Equal("1-3 Yıl", item.ExperienceLevelName);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_WithMissingCompany_ReturnsNullCompanyName_RowStillPresent()
    {
        var summary = CreateSummary(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _personnelNeedModuleContract.SeedGeneralPool(summary);
        // Firma bilinçli olarak seed edilmedi (silinmiş/bulunamayan kayıt senaryosu).

        var result = await CreateHandler().Handle(new GetGeneralPoolQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(summary.Id, item.Id);
        Assert.Null(item.CompanyName);
    }

    [Fact]
    public async Task Handle_WithMissingLookup_ReturnsNullLookupName_RowStillPresent()
    {
        var positionId = Guid.NewGuid();
        var summary = CreateSummary(
            Guid.NewGuid(), positionId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _personnelNeedModuleContract.SeedGeneralPool(summary);
        // Position bilinçli olarak seed edilmedi.

        var result = await CreateHandler().Handle(new GetGeneralPoolQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(summary.Id, item.Id);
        Assert.Null(item.PositionName);
    }

    [Fact]
    public async Task Handle_WithEmptyPool_ReturnsEmptyItems_NoError()
    {
        var result = await CreateHandler().Handle(new GetGeneralPoolQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }
}
