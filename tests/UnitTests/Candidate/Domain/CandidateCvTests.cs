using GenclikMerkezi.Modules.Candidate.Domain;

namespace GenclikMerkezi.UnitTests.Candidate.Domain;

public class CandidateCvTests
{
    private static CandidateCv CreateCv() =>
        CandidateCv.Create(Guid.NewGuid(), "Ahmet", "Yılmaz", "ahmet@example.com", "05551234567");

    [Fact]
    public void Create_SeedsContactInfoFromRegistration()
    {
        var userId = Guid.NewGuid();

        var cv = CandidateCv.Create(userId, "Ahmet", "Yılmaz", "ahmet@example.com", "05551234567");

        Assert.NotEqual(Guid.Empty, cv.Id);
        Assert.Equal(userId, cv.UserId);
        Assert.Equal("Ahmet", cv.FirstName);
        Assert.Equal("Yılmaz", cv.LastName);
        Assert.Equal("ahmet@example.com", cv.Email);
        Assert.Equal("05551234567", cv.PhoneNumber);
        Assert.Equal(0, cv.CompletionPercentage);
        Assert.Null(cv.CountryId);
        Assert.Empty(cv.SocialMediaLinks);
    }

    [Fact]
    public void UpdateContactInfo_CanDivergeFromUserSeedValues()
    {
        var cv = CreateCv();
        var countryId = Guid.NewGuid();
        var provinceId = Guid.NewGuid();
        var districtId = Guid.NewGuid();

        cv.UpdateContactInfo(
            "İş Ahmet", "İş Yılmaz", "is-hesabi@example.com", "05559998877", countryId, provinceId, districtId, "Kadıköy");

        Assert.Equal("İş Ahmet", cv.FirstName);
        Assert.Equal("is-hesabi@example.com", cv.Email);
        Assert.Equal(countryId, cv.CountryId);
        Assert.Equal(provinceId, cv.ProvinceId);
        Assert.Equal(districtId, cv.DistrictId);
        Assert.Equal("Kadıköy", cv.Address);
    }

    [Fact]
    public void UpdatePersonalInfo_SetsAllFields()
    {
        var cv = CreateCv();
        var genderId = Guid.NewGuid();
        var driversLicenseTypeId = Guid.NewGuid();
        var nationalityId = Guid.NewGuid();
        var militaryStatusId = Guid.NewGuid();
        var birthDate = new DateOnly(1998, 5, 20);

        cv.UpdatePersonalInfo("Yazılım Uzmanı", genderId, birthDate, driversLicenseTypeId, nationalityId, 45000m, militaryStatusId);

        Assert.Equal("Yazılım Uzmanı", cv.Title);
        Assert.Equal(genderId, cv.GenderId);
        Assert.Equal(birthDate, cv.BirthDate);
        Assert.Equal(driversLicenseTypeId, cv.DriversLicenseTypeId);
        Assert.Equal(nationalityId, cv.NationalityId);
        Assert.Equal(45000m, cv.NetSalaryExpectation);
        Assert.Equal(militaryStatusId, cv.MilitaryStatusId);
    }

    [Fact]
    public void UpdateDisabilityInfo_WithValue_SetsBlock()
    {
        var cv = CreateCv();
        var disabilityInfo = DisabilityInfo.Create(Guid.NewGuid(), 40, "Açıklama", true, false, true, false, false);

        cv.UpdateDisabilityInfo(disabilityInfo);

        Assert.Equal(disabilityInfo, cv.DisabilityInfo);
    }

    [Fact]
    public void UpdateDisabilityInfo_WithNull_ClearsBlock()
    {
        var cv = CreateCv();
        cv.UpdateDisabilityInfo(DisabilityInfo.Create(Guid.NewGuid(), 40, "Açıklama", true, false, true, false, false));

        cv.UpdateDisabilityInfo(null);

        Assert.Null(cv.DisabilityInfo);
    }

    [Fact]
    public void AddSocialMediaLink_ThenRemove_RemovesIt()
    {
        var cv = CreateCv();
        var link = cv.AddSocialMediaLink("GitHub", "https://github.com/ahmet");
        Assert.Single(cv.SocialMediaLinks);

        cv.RemoveSocialMediaLink(link.Id);

        Assert.Empty(cv.SocialMediaLinks);
    }

    [Fact]
    public void RemoveSocialMediaLink_WithUnknownId_DoesNotThrow()
    {
        var cv = CreateCv();

        var exception = Record.Exception(() => cv.RemoveSocialMediaLink(Guid.NewGuid()));

        Assert.Null(exception);
    }

    [Fact]
    public void AssignCareerAdvisor_SetsId()
    {
        var cv = CreateCv();
        var advisorId = Guid.NewGuid();

        cv.AssignCareerAdvisor(advisorId);

        Assert.Equal(advisorId, cv.CareerAdvisorId);
    }

    [Fact]
    public void UpdateCompletionPercentage_SetsValue()
    {
        var cv = CreateCv();

        cv.UpdateCompletionPercentage(62);

        Assert.Equal(62, cv.CompletionPercentage);
    }
}
