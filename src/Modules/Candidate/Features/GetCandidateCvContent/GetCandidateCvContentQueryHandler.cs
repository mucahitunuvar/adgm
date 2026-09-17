using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;

public sealed class GetCandidateCvContentQueryHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    IFileStorageService fileStorageService,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<GetCandidateCvContentQuery, Result<GetCandidateCvContentResponse>>
{
    public async Task<Result<GetCandidateCvContentResponse>> Handle(
        GetCandidateCvContentQuery request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure<GetCandidateCvContentResponse>(
                Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        if (candidateCv.UserId != currentUserContext.UserId)
        {
            return Result.Failure<GetCandidateCvContentResponse>(
                Error.Forbidden("CandidateCv.NotOwner", "You may only view your own candidate CV content."));
        }

        var content = await candidateCvContentRepository.GetByCandidateCvIdAsync(candidateCv.Id, cancellationToken);

        if (content is null)
        {
            return Result.Failure<GetCandidateCvContentResponse>(
                Error.NotFound("CandidateCvContent.NotFound", "The specified candidate CV content could not be found."));
        }

        var cvFileUrl = content.CvFile is null
            ? null
            : await fileStorageService.GetUrlAsync(content.CvFile.FileKey, cancellationToken);

        var response = new GetCandidateCvContentResponse(
            content.Id,
            content.CandidateCvId,
            content.Summary,
            content.Experiences.Select(e => new ExperienceResponse(
                e.Id, e.CompanyName, e.PositionId, e.StartDate, e.EndDate, e.IsCurrentJob,
                e.SectorId, e.WorkFieldId, e.EmploymentTypeId, e.CountryId, e.ProvinceId, e.JobDescription)).ToList(),
            content.Educations.Select(e => new EducationResponse(
                e.Id, e.EducationLevelId, e.StartDate, e.CompletionStatus.ToString(), e.EndDate,
                e.DiplomaGradingSystemId, e.DiplomaGrade, e.SchoolId, e.SchoolNameFreeText, e.ProvinceId, e.Description)).ToList(),
            content.ComputerSkills,
            content.Languages.Select(l => new CandidateLanguageResponse(l.Id, l.LanguageId, l.LanguageLevelId, l.IsNativeLanguage)).ToList(),
            content.Certificates.Select(c => new CertificateResponse(c.Id, c.Name, c.IssuingInstitution, c.CertificateDate, c.Description)).ToList(),
            content.References.Select(r => new CandidateReferenceResponse(
                r.Id, r.ReferenceTypeId, r.ReferenceLanguageId, r.FirstName, r.LastName, r.Company, r.Position, r.Email, r.PhoneNumber)).ToList(),
            content.Hobbies,
            cvFileUrl);

        return Result.Success(response);
    }
}
