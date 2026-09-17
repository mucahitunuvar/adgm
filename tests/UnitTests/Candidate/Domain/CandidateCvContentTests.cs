using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.UnitTests.Candidate.Domain;

public class CandidateCvContentTests
{
    private static CandidateCvContent CreateContent() => CandidateCvContent.Create(Guid.NewGuid());

    [Fact]
    public void Create_StartsEmpty()
    {
        var candidateCvId = Guid.NewGuid();

        var content = CandidateCvContent.Create(candidateCvId);

        Assert.NotEqual(Guid.Empty, content.Id);
        Assert.Equal(candidateCvId, content.CandidateCvId);
        Assert.Null(content.Summary);
        Assert.Empty(content.Experiences);
        Assert.Empty(content.Educations);
        Assert.Empty(content.Languages);
        Assert.Empty(content.Certificates);
        Assert.Empty(content.References);
    }

    [Fact]
    public void UpdateSummaryComputerSkillsAndHobbies_SetsFields()
    {
        var content = CreateContent();

        content.UpdateSummary("Deneyimli yazılım geliştirici.");
        content.UpdateComputerSkills("C#, SQL Server");
        content.UpdateHobbies("Kitap okumak");

        Assert.Equal("Deneyimli yazılım geliştirici.", content.Summary);
        Assert.Equal("C#, SQL Server", content.ComputerSkills);
        Assert.Equal("Kitap okumak", content.Hobbies);
    }

    [Fact]
    public void AddExperience_ThenRemove_RemovesIt()
    {
        var content = CreateContent();
        var experience = content.AddExperience("Acme A.Ş.", new DateOnly(2020, 1, 1));
        Assert.Single(content.Experiences);

        content.RemoveExperience(experience.Id);

        Assert.Empty(content.Experiences);
    }

    [Fact]
    public void Experience_Update_WithIsCurrentJob_ClearsEndDate()
    {
        var content = CreateContent();
        var experience = content.AddExperience("Acme A.Ş.", new DateOnly(2020, 1, 1));

        experience.Update(
            "Acme A.Ş.",
            positionId: Guid.NewGuid(),
            startDate: new DateOnly(2020, 1, 1),
            endDate: new DateOnly(2022, 1, 1),
            isCurrentJob: true,
            sectorId: null,
            workFieldId: null,
            employmentTypeId: null,
            countryId: null,
            provinceId: null,
            jobDescription: null);

        Assert.True(experience.IsCurrentJob);
        Assert.Null(experience.EndDate);
    }

    [Fact]
    public void AddEducation_ThenRemove_RemovesIt()
    {
        var content = CreateContent();
        var education = content.AddEducation(Guid.NewGuid(), new DateOnly(2016, 9, 1));
        Assert.Single(content.Educations);

        content.RemoveEducation(education.Id);

        Assert.Empty(content.Educations);
    }

    [Fact]
    public void Education_Update_WithDroppedStatus_ClearsEndDateAndDiplomaFields()
    {
        var content = CreateContent();
        var education = content.AddEducation(Guid.NewGuid(), new DateOnly(2016, 9, 1));

        education.Update(
            educationLevelId: Guid.NewGuid(),
            startDate: new DateOnly(2016, 9, 1),
            completionStatus: EducationCompletionStatus.Dropped,
            endDate: new DateOnly(2018, 6, 1),
            diplomaGradingSystemId: Guid.NewGuid(),
            diplomaGrade: 3.2m,
            schoolId: null,
            schoolNameFreeText: "Bilinmeyen Okul",
            provinceId: null,
            description: null);

        Assert.Equal(EducationCompletionStatus.Dropped, education.CompletionStatus);
        Assert.Null(education.EndDate);
        Assert.Null(education.DiplomaGradingSystemId);
        Assert.Null(education.DiplomaGrade);
        Assert.Equal("Bilinmeyen Okul", education.SchoolNameFreeText);
    }

    [Fact]
    public void Education_Update_WithSchoolId_ClearsFreeTextFallback()
    {
        var content = CreateContent();
        var education = content.AddEducation(Guid.NewGuid(), new DateOnly(2016, 9, 1));
        var schoolId = Guid.NewGuid();

        education.Update(
            educationLevelId: Guid.NewGuid(),
            startDate: new DateOnly(2016, 9, 1),
            completionStatus: EducationCompletionStatus.Graduated,
            endDate: new DateOnly(2020, 6, 1),
            diplomaGradingSystemId: null,
            diplomaGrade: null,
            schoolId: schoolId,
            schoolNameFreeText: "Bu Yok Sayılmalı",
            provinceId: null,
            description: null);

        Assert.Equal(schoolId, education.SchoolId);
        Assert.Null(education.SchoolNameFreeText);
    }

    [Fact]
    public void AddLanguage_ThenRemove_RemovesIt()
    {
        var content = CreateContent();
        var language = content.AddLanguage(Guid.NewGuid(), Guid.NewGuid(), isNativeLanguage: true);
        Assert.Single(content.Languages);

        content.RemoveLanguage(language.Id);

        Assert.Empty(content.Languages);
    }

    [Fact]
    public void AddCertificate_ThenRemove_RemovesIt()
    {
        var content = CreateContent();
        var certificate = content.AddCertificate("AWS Certified", "Amazon");
        Assert.Single(content.Certificates);

        content.RemoveCertificate(certificate.Id);

        Assert.Empty(content.Certificates);
    }

    [Fact]
    public void AddReference_ThenRemove_RemovesIt()
    {
        var content = CreateContent();
        var reference = content.AddReference(Guid.NewGuid(), Guid.NewGuid(), "Ayşe", "Kaya");
        Assert.Single(content.References);

        content.RemoveReference(reference.Id);

        Assert.Empty(content.References);
    }

    [Fact]
    public void SetCvFile_SetsAndClears()
    {
        var content = CreateContent();
        var cvFile = FileAttachment.Create(
            "candidate-cv-files/abc.pdf", "cv.pdf", "application/pdf", 1024, DateTime.UtcNow, "CandidateCvContent", content.Id);

        content.SetCvFile(cvFile);
        Assert.Equal(cvFile, content.CvFile);

        content.SetCvFile(null);
        Assert.Null(content.CvFile);
    }
}
