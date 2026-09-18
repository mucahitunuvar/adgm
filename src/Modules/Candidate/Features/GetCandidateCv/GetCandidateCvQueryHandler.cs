using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCv;

// Ownership (AGENTS.md §26): a Candidate may only read their own CV. Admin/CareerAdvisor oversight
// of other candidates' CVs is deferred along with CareerAdvisorId itself (ADR-017 Decision 6 -
// CareerAdvisor module/assignment does not exist yet); the admin/advisor-facing list endpoint
// (SearchCandidates) is the one place that surfaces other candidates today, gated by role instead.
public sealed class GetCandidateCvQueryHandler(
    ICandidateCvRepository candidateCvRepository,
    IFileStorageService fileStorageService,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<GetCandidateCvQuery, Result<GetCandidateCvResponse>>
{
    public async Task<Result<GetCandidateCvResponse>> Handle(GetCandidateCvQuery request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure<GetCandidateCvResponse>(
                Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        if (candidateCv.UserId != currentUserContext.UserId)
        {
            return Result.Failure<GetCandidateCvResponse>(
                Error.Forbidden("CandidateCv.NotOwner", "You may only view your own candidate CV."));
        }

        var photoUrl = candidateCv.Photo is null
            ? null
            : await fileStorageService.GetUrlAsync(candidateCv.Photo.FileKey, cancellationToken);

        var response = new GetCandidateCvResponse(
            candidateCv.Id,
            candidateCv.UserId,
            photoUrl,
            candidateCv.FirstName,
            candidateCv.LastName,
            candidateCv.Email,
            candidateCv.PhoneNumber,
            candidateCv.CountryId,
            candidateCv.ProvinceId,
            candidateCv.DistrictId,
            candidateCv.Address,
            candidateCv.SocialMediaLinks.Select(l => new SocialMediaLinkResponse(l.Id, l.Platform, l.Url)).ToList(),
            candidateCv.Title,
            candidateCv.GenderId,
            candidateCv.BirthDate,
            candidateCv.DriversLicenseTypeId,
            candidateCv.NationalityId,
            candidateCv.NetSalaryExpectation,
            candidateCv.MilitaryStatusId,
            candidateCv.DisabilityInfo is null
                ? null
                : new DisabilityInfoResponse(
                    candidateCv.DisabilityInfo.CategoryId,
                    candidateCv.DisabilityInfo.Percentage,
                    candidateCv.DisabilityInfo.Description,
                    candidateCv.DisabilityInfo.HasHealthReport,
                    candidateCv.DisabilityInfo.UsesMedication,
                    candidateCv.DisabilityInfo.HasChronicCondition,
                    candidateCv.DisabilityInfo.HasContagiousDisease,
                    candidateCv.DisabilityInfo.HasConsciousnessLossRisk),
            candidateCv.CareerAdvisorId,
            candidateCv.CompletionPercentage);

        return Result.Success(response);
    }
}
