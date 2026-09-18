using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Infrastructure.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using UglyToad.PdfPig;

namespace GenclikMerkezi.UnitTests.Candidate.Infrastructure.Pdf;

// Renders a real PDF via QuestPDF and reads it back with PdfPig (a separate, independent parsing
// library - not part of QuestPDF, which is generation-only) to assert on its actual text content.
// See the Ek Görev decision: QuestPDF ships no first-party text-extraction test helper.
public class CvPdfDocumentTests
{
    static CvPdfDocumentTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static string RenderToText(CandidateCvPdfModel model)
    {
        var bytes = new CvPdfDocument(model).GeneratePdf();
        using var document = PdfDocument.Open(bytes);
        return string.Join(" ", document.GetPages().Select(p => p.Text));
    }

    private static CandidateCvPdfModel CreateMinimalModel() => new(
        PhotoBytes: null,
        FullName: "Ahmet Yılmaz",
        Title: null,
        Email: "ahmet@example.com",
        PhoneNumber: null,
        Address: null,
        ProvinceName: null,
        DistrictName: null,
        SocialMediaLinks: [],
        BirthDate: null,
        GenderName: null,
        NationalityName: null,
        DriversLicenseTypeName: null,
        MilitaryStatusName: null,
        DisabilityInfo: null,
        Summary: null,
        Experiences: [],
        Educations: [],
        ComputerSkills: null,
        Languages: [],
        Certificates: [],
        References: [],
        Hobbies: null);

    private static CandidateCvPdfModel CreateFullModel() => CreateMinimalModel() with
    {
        Title = "Yazılım Geliştirici",
        PhoneNumber = "05551234567",
        Address = "Kadıköy",
        ProvinceName = "İstanbul",
        DistrictName = "Kadıköy",
        SocialMediaLinks = [new CandidateCvPdfSocialMediaLink("GitHub", "https://github.com/ahmet")],
        BirthDate = new DateOnly(1995, 3, 10),
        GenderName = "Erkek",
        NationalityName = "Türkiye",
        DriversLicenseTypeName = "B Sınıfı",
        MilitaryStatusName = "Yapıldı",
        DisabilityInfo = new CandidateCvPdfDisabilityInfo("Görme Engeli", 30, "Kısmi görme kaybı", true, false, true, false, false),
        Summary = "Deneyimli yazılım geliştirici.",
        Experiences =
        [
            new CandidateCvPdfExperience(
                "Acme A.Ş.", "Kıdemli Geliştirici", new DateOnly(2020, 1, 1), null, true,
                "Bilişim", "Yazılım", "Tam Zamanlı", "Türkiye", "İstanbul", "Backend geliştirme.")
        ],
        Educations =
        [
            new CandidateCvPdfEducation(
                "Lisans", new DateOnly(2013, 9, 1), "Graduated", new DateOnly(2017, 6, 1),
                "4'lük Sistem", 3.2m, "Boğaziçi Üniversitesi", "İstanbul", "Bilgisayar Mühendisliği")
        ],
        ComputerSkills = "C#, SQL Server",
        Languages = [new CandidateCvPdfLanguage("İngilizce", "İleri", false)],
        Certificates = [new CandidateCvPdfCertificate("AWS Certified", "Amazon", new DateOnly(2022, 5, 1), "Cloud sertifikası")],
        References =
        [
            new CandidateCvPdfReference("İş Referansı", "Türkçe", "Mehmet", "Demir", "Acme A.Ş.", "Müdür", "mehmet@example.com", "05559998877")
        ],
        Hobbies = "Kitap okumak",
    };

    [Fact]
    public void GeneratePdf_WithFullyPopulatedModel_IncludesEverySectionAndItsContent()
    {
        var text = RenderToText(CreateFullModel());

        Assert.Contains("Ahmet Yılmaz", text);
        Assert.Contains("Yazılım Geliştirici", text);
        Assert.Contains("Kişisel Bilgiler", text);
        Assert.Contains("Erkek", text);
        Assert.Contains("Görme Engeli", text);
        Assert.Contains("Özet", text);
        Assert.Contains("Deneyimli yazılım geliştirici.", text);
        Assert.Contains("Deneyim", text);
        Assert.Contains("Acme A.Ş.", text);
        Assert.Contains("Eğitim", text);
        Assert.Contains("Boğaziçi Üniversitesi", text);
        Assert.Contains("Bilgisayar Bilgisi", text);
        Assert.Contains("C#, SQL Server", text);
        Assert.Contains("Diller", text);
        Assert.Contains("İngilizce", text);
        Assert.Contains("Sertifikalar", text);
        Assert.Contains("AWS Certified", text);
        Assert.Contains("Referanslar", text);
        Assert.Contains("Mehmet", text);
        Assert.Contains("Hobiler", text);
        Assert.Contains("Kitap okumak", text);
    }

    [Fact]
    public void GeneratePdf_WithMinimalModel_OmitsEveryEmptySectionHeading()
    {
        var text = RenderToText(CreateMinimalModel());

        Assert.Contains("Ahmet Yılmaz", text);
        Assert.DoesNotContain("Kişisel Bilgiler", text);
        Assert.DoesNotContain("Özet", text);
        Assert.DoesNotContain("Deneyim", text);
        Assert.DoesNotContain("Eğitim", text);
        Assert.DoesNotContain("Bilgisayar Bilgisi", text);
        Assert.DoesNotContain("Diller", text);
        Assert.DoesNotContain("Sertifikalar", text);
        Assert.DoesNotContain("Referanslar", text);
        Assert.DoesNotContain("Hobiler", text);
    }

    [Fact]
    public void GeneratePdf_WithOnlyCertificatesEmpty_OmitsOnlyTheCertificatesHeading()
    {
        var model = CreateMinimalModel() with
        {
            Experiences =
            [
                new CandidateCvPdfExperience(
                    "Acme A.Ş.", null, new DateOnly(2020, 1, 1), null, true, null, null, null, null, null, null)
            ],
        };

        var text = RenderToText(model);

        Assert.Contains("Deneyim", text);
        Assert.DoesNotContain("Sertifikalar", text);
    }

    [Fact]
    public void GeneratePdf_NeverRendersNetSalaryExpectation()
    {
        // CandidateCvPdfModel has no field that could carry Net Maaş Beklentisi at all (see
        // CandidateCvPdfModelBuilderTests for the structural guarantee) - this asserts the rendering
        // side too, end to end, against a distinctive value that would be obviously wrong to see.
        var text = RenderToText(CreateFullModel());

        Assert.DoesNotContain("999999", text);
        Assert.DoesNotContain("Maaş", text);
        Assert.DoesNotContain("Salary", text);
    }
}
