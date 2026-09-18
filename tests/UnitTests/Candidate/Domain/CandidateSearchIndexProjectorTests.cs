using GenclikMerkezi.Modules.Candidate.Domain;

namespace GenclikMerkezi.UnitTests.Candidate.Domain;

public class CandidateSearchIndexProjectorTests
{
    private static CandidateCv CreateCv() =>
        CandidateCv.Create(Guid.NewGuid(), "İrem", "Yılmaz", "irem@example.com", null);

    [Fact]
    public void CreateInitial_ProjectsNameAndEmail_WithEmptyCollectionsAndZeroPercentage()
    {
        var cv = CreateCv();
        var updatedAtUtc = new DateTime(2026, 9, 18, 10, 0, 0, DateTimeKind.Utc);

        var index = CandidateSearchIndexProjector.CreateInitial(cv, updatedAtUtc);

        Assert.Equal(cv.Id, index.Id);
        Assert.Equal("IREM YILMAZ", index.FullNameNormalized);
        Assert.Equal("irem@example.com", index.Email);
        Assert.Equal(0, index.CompletionPercentage);
        Assert.Empty(index.EducationLevelIds);
        Assert.Empty(index.SectorIds);
        Assert.Equal(updatedAtUtc, index.UpdatedAtUtc);
    }

    [Fact]
    public void Project_WithExperiencesAndEducations_PopulatesDistinctSectorAndEducationLevelIds()
    {
        var cv = CreateCv();
        cv.UpdateCompletionPercentage(42);
        var index = CandidateSearchIndexProjector.CreateInitial(cv, DateTime.UtcNow);

        var content = CandidateCvContent.Create(cv.Id);
        var sectorId = Guid.NewGuid();
        content.AddExperience("Acme A.Ş.", new DateOnly(2020, 1, 1));
        var firstExperience = content.Experiences.First();
        firstExperience.Update(
            "Acme A.Ş.", null, new DateOnly(2020, 1, 1), null, true, sectorId, null, null, null, null, null);

        // A second experience in the same sector should not duplicate the id.
        content.AddExperience("Second A.Ş.", new DateOnly(2021, 1, 1));
        content.Experiences.Last().Update(
            "Second A.Ş.", null, new DateOnly(2021, 1, 1), null, true, sectorId, null, null, null, null, null);

        var educationLevelId = Guid.NewGuid();
        content.AddEducation(educationLevelId, new DateOnly(2016, 9, 1));

        var updatedAtUtc = new DateTime(2026, 9, 18, 12, 0, 0, DateTimeKind.Utc);
        CandidateSearchIndexProjector.Project(index, cv, content, updatedAtUtc);

        Assert.Equal(42, index.CompletionPercentage);
        Assert.Equal([sectorId], index.SectorIds);
        Assert.Equal([educationLevelId], index.EducationLevelIds);
        Assert.Equal(updatedAtUtc, index.UpdatedAtUtc);
    }

    [Fact]
    public void Project_WithExperienceWithoutSector_DoesNotAddNullToSectorIds()
    {
        var cv = CreateCv();
        var index = CandidateSearchIndexProjector.CreateInitial(cv, DateTime.UtcNow);

        var content = CandidateCvContent.Create(cv.Id);
        content.AddExperience("Acme A.Ş.", new DateOnly(2020, 1, 1));

        CandidateSearchIndexProjector.Project(index, cv, content, DateTime.UtcNow);

        Assert.Empty(index.SectorIds);
    }

    [Fact]
    public void Project_WithNullContent_KeepsEducationAndSectorIdsEmpty()
    {
        var cv = CreateCv();
        var index = CandidateSearchIndexProjector.CreateInitial(cv, DateTime.UtcNow);

        CandidateSearchIndexProjector.Project(index, cv, candidateCvContent: null, DateTime.UtcNow);

        Assert.Empty(index.EducationLevelIds);
        Assert.Empty(index.SectorIds);
    }

    [Fact]
    public void Project_ReflectsUpdatedContactInfo_InFullNameNormalizedAndEmail()
    {
        var cv = CreateCv();
        var index = CandidateSearchIndexProjector.CreateInitial(cv, DateTime.UtcNow);

        cv.UpdateContactInfo("Ayşe", "Çelik", "ayse@example.com", null, null, null, null, null);

        CandidateSearchIndexProjector.Project(index, cv, candidateCvContent: null, DateTime.UtcNow);

        Assert.Equal("AYSE CELIK", index.FullNameNormalized);
        Assert.Equal("ayse@example.com", index.Email);
    }
}
