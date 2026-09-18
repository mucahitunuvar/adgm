using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.SearchCandidates;

public sealed class SearchCandidatesQueryHandler(
    ICandidateSearchIndexRepository candidateSearchIndexRepository, IReferenceDataLookupReader referenceDataLookupReader)
    : IRequestHandler<SearchCandidatesQuery, Result<SearchCandidatesResponse>>
{
    public async Task<Result<SearchCandidatesResponse>> Handle(SearchCandidatesQuery request, CancellationToken cancellationToken)
    {
        var filter = new CandidateSearchIndexFilter(
            request.SearchText,
            request.ProvinceId,
            request.DistrictId,
            request.EducationLevelId,
            request.SectorId,
            request.MinCompletionPercentage,
            request.Page,
            request.PageSize);

        var pagedCandidates = await candidateSearchIndexRepository.SearchAsync(filter, cancellationToken);

        // Batch-resolved for this one page only (ADR-016) - never store Province/District display
        // names on CandidateSearchIndex itself, they belong to ReferenceData.
        var provinceIds = pagedCandidates.Items
            .Where(c => c.ProvinceId is not null)
            .Select(c => c.ProvinceId!.Value)
            .Distinct()
            .ToArray();
        var districtIds = pagedCandidates.Items
            .Where(c => c.DistrictId is not null)
            .Select(c => c.DistrictId!.Value)
            .Distinct()
            .ToArray();

        var provinceNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.Province, provinceIds, cancellationToken))
            .ToDictionary(p => p.Id, p => p.DisplayName);
        var districtNamesById = (await referenceDataLookupReader.GetByIdsAsync(
                ReferenceDataLookupType.District, districtIds, cancellationToken))
            .ToDictionary(d => d.Id, d => d.DisplayName);

        var items = pagedCandidates.Items
            .Select(c => new CandidateListItemResponse(
                c.CandidateCvId,
                $"{c.FirstName} {c.LastName}",
                c.Email,
                c.ProvinceId is not null && provinceNamesById.TryGetValue(c.ProvinceId.Value, out var provinceName)
                    ? provinceName
                    : null,
                c.DistrictId is not null && districtNamesById.TryGetValue(c.DistrictId.Value, out var districtName)
                    ? districtName
                    : null,
                c.CompletionPercentage))
            .ToList();

        return Result.Success(new SearchCandidatesResponse(
            items,
            pagedCandidates.TotalCount,
            pagedCandidates.Page,
            pagedCandidates.PageSize,
            pagedCandidates.TotalPages,
            pagedCandidates.HasNextPage,
            pagedCandidates.HasPreviousPage));
    }
}
