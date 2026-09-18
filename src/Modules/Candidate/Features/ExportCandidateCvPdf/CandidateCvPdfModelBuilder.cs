using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;

namespace GenclikMerkezi.Modules.Candidate.Features.ExportCandidateCvPdf;

// Pure mapping (no I/O) from the domain aggregates + a pre-resolved lookup-name dictionary into
// CandidateCvPdfModel (ADR-021). Kept separate from the handler so both halves - "which ids does
// this CV reference" and "how do they turn into the PDF's shape" - stay unit-testable without a
// database, file storage, or ReferenceData.
public static class CandidateCvPdfModelBuilder
{
    // Every distinct (lookup type, id) the CV actually references, grouped by type so the handler
    // can resolve each type with a single batched IReferenceDataLookupReader.GetByIdsAsync call.
    public static IReadOnlyDictionary<ReferenceDataLookupType, HashSet<Guid>> CollectLookupIds(
        CandidateCv candidateCv, CandidateCvContent? candidateCvContent)
    {
        var idsByType = new Dictionary<ReferenceDataLookupType, HashSet<Guid>>();

        void Track(ReferenceDataLookupType type, Guid? id)
        {
            if (id is null)
            {
                return;
            }

            if (!idsByType.TryGetValue(type, out var ids))
            {
                ids = [];
                idsByType[type] = ids;
            }

            ids.Add(id.Value);
        }

        Track(ReferenceDataLookupType.Province, candidateCv.ProvinceId);
        Track(ReferenceDataLookupType.District, candidateCv.DistrictId);
        Track(ReferenceDataLookupType.Nationality, candidateCv.NationalityId);
        Track(ReferenceDataLookupType.Gender, candidateCv.GenderId);
        Track(ReferenceDataLookupType.DriversLicenseType, candidateCv.DriversLicenseTypeId);
        Track(ReferenceDataLookupType.MilitaryStatus, candidateCv.MilitaryStatusId);
        Track(ReferenceDataLookupType.DisabilityCategory, candidateCv.DisabilityInfo?.CategoryId);

        foreach (var experience in candidateCvContent?.Experiences ?? [])
        {
            Track(ReferenceDataLookupType.Position, experience.PositionId);
            Track(ReferenceDataLookupType.Sector, experience.SectorId);
            Track(ReferenceDataLookupType.WorkField, experience.WorkFieldId);
            Track(ReferenceDataLookupType.EmploymentType, experience.EmploymentTypeId);
            Track(ReferenceDataLookupType.Country, experience.CountryId);
            Track(ReferenceDataLookupType.Province, experience.ProvinceId);
        }

        foreach (var education in candidateCvContent?.Educations ?? [])
        {
            Track(ReferenceDataLookupType.EducationLevel, education.EducationLevelId);
            Track(ReferenceDataLookupType.DiplomaGradingSystem, education.DiplomaGradingSystemId);
            Track(ReferenceDataLookupType.School, education.SchoolId);
            Track(ReferenceDataLookupType.Province, education.ProvinceId);
        }

        foreach (var language in candidateCvContent?.Languages ?? [])
        {
            Track(ReferenceDataLookupType.Language, language.LanguageId);
            Track(ReferenceDataLookupType.LanguageLevel, language.LanguageLevelId);
        }

        foreach (var reference in candidateCvContent?.References ?? [])
        {
            Track(ReferenceDataLookupType.ReferenceType, reference.ReferenceTypeId);
            Track(ReferenceDataLookupType.Language, reference.ReferenceLanguageId);
        }

        return idsByType;
    }

    public static CandidateCvPdfModel Build(
        CandidateCv candidateCv,
        CandidateCvContent? candidateCvContent,
        byte[]? photoBytes,
        IReadOnlyDictionary<(ReferenceDataLookupType Type, Guid Id), string> lookupNames)
    {
        string? Resolve(ReferenceDataLookupType type, Guid? id) =>
            id is not null && lookupNames.TryGetValue((type, id.Value), out var name) ? name : null;

        return new CandidateCvPdfModel(
            photoBytes,
            $"{candidateCv.FirstName} {candidateCv.LastName}",
            candidateCv.Title,
            candidateCv.Email,
            candidateCv.PhoneNumber,
            candidateCv.Address,
            Resolve(ReferenceDataLookupType.Province, candidateCv.ProvinceId),
            Resolve(ReferenceDataLookupType.District, candidateCv.DistrictId),
            candidateCv.SocialMediaLinks.Select(l => new CandidateCvPdfSocialMediaLink(l.Platform, l.Url)).ToList(),
            candidateCv.BirthDate,
            Resolve(ReferenceDataLookupType.Gender, candidateCv.GenderId),
            Resolve(ReferenceDataLookupType.Nationality, candidateCv.NationalityId),
            Resolve(ReferenceDataLookupType.DriversLicenseType, candidateCv.DriversLicenseTypeId),
            Resolve(ReferenceDataLookupType.MilitaryStatus, candidateCv.MilitaryStatusId),
            BuildDisabilityInfo(candidateCv.DisabilityInfo, Resolve),
            candidateCvContent?.Summary,
            (candidateCvContent?.Experiences ?? []).Select(e => BuildExperience(e, Resolve)).ToList(),
            (candidateCvContent?.Educations ?? []).Select(e => BuildEducation(e, Resolve)).ToList(),
            candidateCvContent?.ComputerSkills,
            (candidateCvContent?.Languages ?? []).Select(l => BuildLanguage(l, Resolve)).ToList(),
            (candidateCvContent?.Certificates ?? [])
                .Select(c => new CandidateCvPdfCertificate(c.Name, c.IssuingInstitution, c.CertificateDate, c.Description))
                .ToList(),
            (candidateCvContent?.References ?? []).Select(r => BuildReference(r, Resolve)).ToList(),
            candidateCvContent?.Hobbies);
    }

    private static CandidateCvPdfDisabilityInfo? BuildDisabilityInfo(
        DisabilityInfo? disabilityInfo, Func<ReferenceDataLookupType, Guid?, string?> resolve)
    {
        if (disabilityInfo is null)
        {
            return null;
        }

        return new CandidateCvPdfDisabilityInfo(
            resolve(ReferenceDataLookupType.DisabilityCategory, disabilityInfo.CategoryId) ?? string.Empty,
            disabilityInfo.Percentage,
            disabilityInfo.Description,
            disabilityInfo.HasHealthReport,
            disabilityInfo.UsesMedication,
            disabilityInfo.HasChronicCondition,
            disabilityInfo.HasContagiousDisease,
            disabilityInfo.HasConsciousnessLossRisk);
    }

    private static CandidateCvPdfExperience BuildExperience(
        Experience experience, Func<ReferenceDataLookupType, Guid?, string?> resolve) =>
        new(
            experience.CompanyName,
            resolve(ReferenceDataLookupType.Position, experience.PositionId),
            experience.StartDate,
            experience.EndDate,
            experience.IsCurrentJob,
            resolve(ReferenceDataLookupType.Sector, experience.SectorId),
            resolve(ReferenceDataLookupType.WorkField, experience.WorkFieldId),
            resolve(ReferenceDataLookupType.EmploymentType, experience.EmploymentTypeId),
            resolve(ReferenceDataLookupType.Country, experience.CountryId),
            resolve(ReferenceDataLookupType.Province, experience.ProvinceId),
            experience.JobDescription);

    private static CandidateCvPdfEducation BuildEducation(
        Education education, Func<ReferenceDataLookupType, Guid?, string?> resolve) =>
        new(
            resolve(ReferenceDataLookupType.EducationLevel, education.EducationLevelId) ?? string.Empty,
            education.StartDate,
            education.CompletionStatus.ToString(),
            education.EndDate,
            resolve(ReferenceDataLookupType.DiplomaGradingSystem, education.DiplomaGradingSystemId),
            education.DiplomaGrade,
            education.SchoolId is not null
                ? resolve(ReferenceDataLookupType.School, education.SchoolId)
                : education.SchoolNameFreeText,
            resolve(ReferenceDataLookupType.Province, education.ProvinceId),
            education.Description);

    private static CandidateCvPdfLanguage BuildLanguage(
        CandidateLanguage language, Func<ReferenceDataLookupType, Guid?, string?> resolve) =>
        new(
            resolve(ReferenceDataLookupType.Language, language.LanguageId) ?? string.Empty,
            resolve(ReferenceDataLookupType.LanguageLevel, language.LanguageLevelId) ?? string.Empty,
            language.IsNativeLanguage);

    private static CandidateCvPdfReference BuildReference(
        CandidateReference reference, Func<ReferenceDataLookupType, Guid?, string?> resolve) =>
        new(
            resolve(ReferenceDataLookupType.ReferenceType, reference.ReferenceTypeId) ?? string.Empty,
            resolve(ReferenceDataLookupType.Language, reference.ReferenceLanguageId) ?? string.Empty,
            reference.FirstName,
            reference.LastName,
            reference.Company,
            reference.Position,
            reference.Email,
            reference.PhoneNumber);
}
