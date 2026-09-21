using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.GetGeneralPool;

// SearchCandidatesQueryHandler deseni: bir sayfalık PersonnelNeedSummary'nin distinct id kümeleri
// için toplu (tek tek değil) çözüm, sonra Dictionary + TryGetValue ile projeksiyon. Firma adı
// ICompanyModuleContract üzerinden, altı lookup adı (Position/Department/Province/EmploymentType/
// WorkLocationType/ExperienceLevel) IReferenceDataLookupReader üzerinden, her biri kendi tipinde
// ayrı bir GetByIdsAsync çağrısıyla çözülür.
public sealed class GetGeneralPoolQueryHandler(
    IPersonnelNeedModuleContract personnelNeedModuleContract,
    ICompanyModuleContract companyModuleContract,
    IReferenceDataLookupReader referenceDataLookupReader)
    : IRequestHandler<GetGeneralPoolQuery, Result<GetGeneralPoolResponse>>
{
    public async Task<Result<GetGeneralPoolResponse>> Handle(GetGeneralPoolQuery request, CancellationToken cancellationToken)
    {
        var pagedPool = await personnelNeedModuleContract.GetGeneralPoolAsync(request, cancellationToken);

        var companyIds = pagedPool.Items.Select(p => p.CompanyId).Distinct().ToArray();
        var positionIds = pagedPool.Items.Select(p => p.PositionId).Distinct().ToArray();
        var departmentIds = pagedPool.Items.Select(p => p.DepartmentId).Distinct().ToArray();
        var provinceIds = pagedPool.Items.Select(p => p.ProvinceId).Distinct().ToArray();
        var employmentTypeIds = pagedPool.Items.Select(p => p.EmploymentTypeId).Distinct().ToArray();
        var workLocationTypeIds = pagedPool.Items.Select(p => p.WorkLocationTypeId).Distinct().ToArray();
        var experienceLevelIds = pagedPool.Items.Select(p => p.ExperienceLevelId).Distinct().ToArray();

        var companyNamesById = (await companyModuleContract.GetCompaniesByIdsAsync(companyIds, cancellationToken))
            .ToDictionary(c => c.Id, c => c.Name);
        var positionNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.Position, positionIds, cancellationToken))
            .ToDictionary(l => l.Id, l => l.DisplayName);
        var departmentNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.Department, departmentIds, cancellationToken))
            .ToDictionary(l => l.Id, l => l.DisplayName);
        var provinceNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.Province, provinceIds, cancellationToken))
            .ToDictionary(l => l.Id, l => l.DisplayName);
        var employmentTypeNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.EmploymentType, employmentTypeIds, cancellationToken))
            .ToDictionary(l => l.Id, l => l.DisplayName);
        var workLocationTypeNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.WorkLocationType, workLocationTypeIds, cancellationToken))
            .ToDictionary(l => l.Id, l => l.DisplayName);
        var experienceLevelNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.ExperienceLevel, experienceLevelIds, cancellationToken))
            .ToDictionary(l => l.Id, l => l.DisplayName);

        var items = pagedPool.Items
            .Select(p => new PersonnelNeedPoolItemResponse(
                p.Id,
                companyNamesById.TryGetValue(p.CompanyId, out var companyName) ? companyName : null,
                positionNamesById.TryGetValue(p.PositionId, out var positionName) ? positionName : null,
                departmentNamesById.TryGetValue(p.DepartmentId, out var departmentName) ? departmentName : null,
                provinceNamesById.TryGetValue(p.ProvinceId, out var provinceName) ? provinceName : null,
                employmentTypeNamesById.TryGetValue(p.EmploymentTypeId, out var employmentTypeName) ? employmentTypeName : null,
                workLocationTypeNamesById.TryGetValue(p.WorkLocationTypeId, out var workLocationTypeName) ? workLocationTypeName : null,
                experienceLevelNamesById.TryGetValue(p.ExperienceLevelId, out var experienceLevelName) ? experienceLevelName : null,
                p.Quantity,
                p.DetailsText,
                p.PooledAtUtc))
            .ToList();

        return Result.Success(new GetGeneralPoolResponse(
            items,
            pagedPool.TotalCount,
            pagedPool.Page,
            pagedPool.PageSize,
            pagedPool.TotalPages,
            pagedPool.HasNextPage,
            pagedPool.HasPreviousPage));
    }
}
