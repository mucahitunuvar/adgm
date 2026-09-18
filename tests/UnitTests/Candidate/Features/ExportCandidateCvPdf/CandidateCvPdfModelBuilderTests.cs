using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.Modules.Candidate.Features.ExportCandidateCvPdf;

namespace GenclikMerkezi.UnitTests.Candidate.Features.ExportCandidateCvPdf;

public class CandidateCvPdfModelBuilderTests
{
    private static CandidateCv CreateCv() =>
        CandidateCv.Create(Guid.NewGuid(), "Ahmet", "Yılmaz", "ahmet@example.com", "05551234567");

    [Fact]
    public void CollectLookupIds_WithFullyPopulatedCv_CollectsEveryDistinctIdGroupedByType()
    {
        var provinceId = Guid.NewGuid();
        var nationalityId = Guid.NewGuid();
        var cv = CreateCv();
        cv.UpdateContactInfo("Ahmet", "Yılmaz", "ahmet@example.com", null, null, provinceId, null, null);
        cv.UpdatePersonalInfo(null, null, null, null, nationalityId, null, null);

        var content = CandidateCvContent.Create(cv.Id);
        var sectorId = Guid.NewGuid();
        content.AddExperience("Acme A.Ş.", new DateOnly(2020, 1, 1));
        content.Experiences.First().Update(
            "Acme A.Ş.", null, new DateOnly(2020, 1, 1), null, true, sectorId, null, null, null, provinceId, null);

        var educationLevelId = Guid.NewGuid();
        content.AddEducation(educationLevelId, new DateOnly(2016, 9, 1));

        var languageId = Guid.NewGuid();
        var languageLevelId = Guid.NewGuid();
        content.AddLanguage(languageId, languageLevelId, isNativeLanguage: false);

        var result = CandidateCvPdfModelBuilder.CollectLookupIds(cv, content);

        Assert.Equal([provinceId], result[ReferenceDataLookupType.Province]);
        Assert.Equal([nationalityId], result[ReferenceDataLookupType.Nationality]);
        Assert.Equal([sectorId], result[ReferenceDataLookupType.Sector]);
        Assert.Equal([educationLevelId], result[ReferenceDataLookupType.EducationLevel]);
        Assert.Equal([languageId], result[ReferenceDataLookupType.Language]);
        Assert.Equal([languageLevelId], result[ReferenceDataLookupType.LanguageLevel]);
    }

    [Fact]
    public void CollectLookupIds_WithNoContent_OnlyCollectsCandidateCvOwnIds()
    {
        var result = CandidateCvPdfModelBuilder.CollectLookupIds(CreateCv(), candidateCvContent: null);

        Assert.Empty(result);
    }

    [Fact]
    public void Build_ResolvesEveryLookupIdToItsName_UsingTheSuppliedDictionary()
    {
        var provinceId = Guid.NewGuid();
        var cv = CreateCv();
        cv.UpdateContactInfo("Ahmet", "Yılmaz", "ahmet@example.com", null, null, provinceId, null, "Kadıköy");

        var lookupNames = new Dictionary<(ReferenceDataLookupType, Guid), string>
        {
            [(ReferenceDataLookupType.Province, provinceId)] = "İstanbul",
        };

        var model = CandidateCvPdfModelBuilder.Build(cv, candidateCvContent: null, photoBytes: null, lookupNames);

        Assert.Equal("Ahmet Yılmaz", model.FullName);
        Assert.Equal("İstanbul", model.ProvinceName);
        Assert.Equal("Kadıköy", model.Address);
    }

    [Fact]
    public void Build_WithUnresolvableLookupId_LeavesNamePropertyNull()
    {
        var cv = CreateCv();
        cv.UpdateContactInfo("Ahmet", "Yılmaz", "ahmet@example.com", null, null, Guid.NewGuid(), null, null);

        var model = CandidateCvPdfModelBuilder.Build(
            cv, candidateCvContent: null, photoBytes: null, new Dictionary<(ReferenceDataLookupType, Guid), string>());

        Assert.Null(model.ProvinceName);
    }

    [Fact]
    public void Build_WithSchoolId_PrefersResolvedSchoolNameOverFreeText()
    {
        var cv = CreateCv();
        var content = CandidateCvContent.Create(cv.Id);
        var schoolId = Guid.NewGuid();
        var educationLevelId = Guid.NewGuid();
        content.AddEducation(educationLevelId, new DateOnly(2016, 9, 1));
        content.Educations.First().Update(
            educationLevelId, new DateOnly(2016, 9, 1), EducationCompletionStatus.Graduated, new DateOnly(2020, 6, 1),
            null, null, schoolId, "Serbest Metin Okul Adı", null, null);

        var lookupNames = new Dictionary<(ReferenceDataLookupType, Guid), string>
        {
            [(ReferenceDataLookupType.School, schoolId)] = "Kadıköy Anadolu Lisesi",
            [(ReferenceDataLookupType.EducationLevel, educationLevelId)] = "Lise",
        };

        var model = CandidateCvPdfModelBuilder.Build(cv, content, photoBytes: null, lookupNames);

        var education = Assert.Single(model.Educations);
        Assert.Equal("Kadıköy Anadolu Lisesi", education.SchoolName);
        Assert.Equal("Graduated", education.CompletionStatus);
    }

    [Fact]
    public void Build_WithoutSchoolId_FallsBackToFreeTextSchoolName()
    {
        var cv = CreateCv();
        var content = CandidateCvContent.Create(cv.Id);
        var educationLevelId = Guid.NewGuid();
        content.AddEducation(educationLevelId, new DateOnly(2016, 9, 1));
        content.Educations.First().Update(
            educationLevelId, new DateOnly(2016, 9, 1), EducationCompletionStatus.Continuing, null,
            null, null, null, "Bilinmeyen Bir Lise", null, null);

        var model = CandidateCvPdfModelBuilder.Build(
            cv, content, photoBytes: null, new Dictionary<(ReferenceDataLookupType, Guid), string>());

        var education = Assert.Single(model.Educations);
        Assert.Equal("Bilinmeyen Bir Lise", education.SchoolName);
    }

    [Fact]
    public void Build_NeverIncludesNetSalaryExpectation_AnywhereInTheModel()
    {
        var cv = CreateCv();
        cv.UpdatePersonalInfo(null, null, null, null, null, 999999.99m, null);

        var model = CandidateCvPdfModelBuilder.Build(
            cv, candidateCvContent: null, photoBytes: null, new Dictionary<(ReferenceDataLookupType, Guid), string>());

        // Structural guarantee, not just a missing-field check: CandidateCvPdfModel has no property
        // that could carry it, so there is no way for this handler/builder to leak it into the PDF.
        var modelProperties = typeof(GenclikMerkezi.Modules.Candidate.Application.Abstractions.CandidateCvPdfModel)
            .GetProperties()
            .Select(p => p.Name);
        Assert.DoesNotContain(modelProperties, name => name.Contains("Salary", StringComparison.OrdinalIgnoreCase));
        _ = model;
    }
}
