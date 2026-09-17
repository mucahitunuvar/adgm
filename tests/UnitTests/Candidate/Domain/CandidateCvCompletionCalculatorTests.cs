using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.UnitTests.Candidate.Domain;

public class CandidateCvCompletionCalculatorTests
{
    private static CandidateCv CreateCv() =>
        CandidateCv.Create(Guid.NewGuid(), "Ahmet", "Yılmaz", "ahmet@example.com", null);

    private static FileAttachment CreateFile(Guid ownerId, string ownerType) =>
        FileAttachment.Create("key", "file.pdf", "application/pdf", 1024, DateTime.UtcNow, ownerType, ownerId);

    [Fact]
    public void Calculate_WithNoCriteriaMetAndNoContent_ReturnsZero()
    {
        var cv = CreateCv();

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, candidateCvContent: null);

        Assert.Equal(0, percentage);
    }

    [Fact]
    public void Calculate_WithAllEightCriteriaMet_ReturnsOneHundred()
    {
        var cv = CreateCv();
        cv.SetPhoto(CreateFile(cv.Id, "CandidateCv"));
        cv.UpdateContactInfo("Ahmet", "Yılmaz", "ahmet@example.com", null, null, null, null, "Kadıköy, İstanbul");
        cv.AddSocialMediaLink("GitHub", "https://github.com/ahmet");

        var content = CandidateCvContent.Create(cv.Id);
        content.UpdateSummary("Deneyimli yazılım geliştirici.");
        content.AddExperience("Acme A.Ş.", new DateOnly(2020, 1, 1));
        content.AddEducation(Guid.NewGuid(), new DateOnly(2016, 9, 1));
        content.AddLanguage(Guid.NewGuid(), Guid.NewGuid(), isNativeLanguage: true);
        content.SetCvFile(CreateFile(content.Id, "CandidateCvContent"));

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, content);

        Assert.Equal(100, percentage);
    }

    [Fact]
    public void Calculate_WithPhotoOnly_ReturnsOneOfEight()
    {
        var cv = CreateCv();
        cv.SetPhoto(CreateFile(cv.Id, "CandidateCv"));

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, candidateCvContent: null);

        Assert.Equal(12, percentage); // Round(1/8 * 100) = 12 (Math.Round banker's rounding of 12.5)
    }

    [Fact]
    public void Calculate_WithAddressOnly_ReturnsOneOfEight()
    {
        var cv = CreateCv();
        cv.UpdateContactInfo("Ahmet", "Yılmaz", "ahmet@example.com", null, null, null, null, "Kadıköy, İstanbul");

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, candidateCvContent: null);

        Assert.Equal(12, percentage);
    }

    [Fact]
    public void Calculate_WithSocialMediaLinkOnly_ReturnsOneOfEight()
    {
        var cv = CreateCv();
        cv.AddSocialMediaLink("GitHub", "https://github.com/ahmet");

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, candidateCvContent: null);

        Assert.Equal(12, percentage);
    }

    [Fact]
    public void Calculate_WithSummaryOnly_ReturnsOneOfEight()
    {
        var cv = CreateCv();
        var content = CandidateCvContent.Create(cv.Id);
        content.UpdateSummary("Deneyimli yazılım geliştirici.");

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, content);

        Assert.Equal(12, percentage);
    }

    [Fact]
    public void Calculate_WithOneExperienceOnly_ReturnsOneOfEight()
    {
        var cv = CreateCv();
        var content = CandidateCvContent.Create(cv.Id);
        content.AddExperience("Acme A.Ş.", new DateOnly(2020, 1, 1));

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, content);

        Assert.Equal(12, percentage);
    }

    [Fact]
    public void Calculate_WithOneEducationOnly_ReturnsOneOfEight()
    {
        var cv = CreateCv();
        var content = CandidateCvContent.Create(cv.Id);
        content.AddEducation(Guid.NewGuid(), new DateOnly(2016, 9, 1));

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, content);

        Assert.Equal(12, percentage);
    }

    [Fact]
    public void Calculate_WithOneLanguageOnly_ReturnsOneOfEight()
    {
        var cv = CreateCv();
        var content = CandidateCvContent.Create(cv.Id);
        content.AddLanguage(Guid.NewGuid(), Guid.NewGuid(), isNativeLanguage: true);

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, content);

        Assert.Equal(12, percentage);
    }

    [Fact]
    public void Calculate_WithCvFileOnly_ReturnsOneOfEight()
    {
        var cv = CreateCv();
        var content = CandidateCvContent.Create(cv.Id);
        content.SetCvFile(CreateFile(content.Id, "CandidateCvContent"));

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, content);

        Assert.Equal(12, percentage);
    }

    [Fact]
    public void Calculate_WithExactlyHalfCriteriaMet_ReturnsFifty()
    {
        var cv = CreateCv();
        cv.SetPhoto(CreateFile(cv.Id, "CandidateCv"));
        cv.UpdateContactInfo("Ahmet", "Yılmaz", "ahmet@example.com", null, null, null, null, "Kadıköy, İstanbul");
        cv.AddSocialMediaLink("GitHub", "https://github.com/ahmet");

        var content = CandidateCvContent.Create(cv.Id);
        content.UpdateSummary("Deneyimli yazılım geliştirici.");

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, content);

        Assert.Equal(50, percentage);
    }

    [Fact]
    public void Calculate_EmptySummary_DoesNotCountAsMet()
    {
        var cv = CreateCv();
        var content = CandidateCvContent.Create(cv.Id);
        content.UpdateSummary("   ");

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, content);

        Assert.Equal(0, percentage);
    }

    [Fact]
    public void Calculate_EmptyAddress_DoesNotCountAsMet()
    {
        var cv = CreateCv();
        cv.UpdateContactInfo("Ahmet", "Yılmaz", "ahmet@example.com", null, null, null, null, address: "   ");

        var percentage = CandidateCvCompletionCalculator.Calculate(cv, candidateCvContent: null);

        Assert.Equal(0, percentage);
    }
}
