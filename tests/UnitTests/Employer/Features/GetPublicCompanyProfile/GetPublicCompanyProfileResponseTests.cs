using GenclikMerkezi.Modules.Employer.Features.GetPublicCompanyProfile;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetPublicCompanyProfile;

// Görev 2 (Employer public jobs master prompt) Testler: "yanıtta yasaklı alanların bulunmaması
// (yanıt tipinin alan listesi testi)" - iletişim, vergi, danışman, inceleme/ret notu hiçbir public
// yanıtta yer almaz. Bu bir reflection testi: yeni bir alan eklenirse ve yasaklı bir isimle
// çakışırsa derleme zamanında değil burada yakalanır.
public class GetPublicCompanyProfileResponseTests
{
    private static readonly string[] ForbiddenPropertyNames =
    [
        "ContactFirstName", "ContactLastName", "ContactEmail", "ContactPhone",
        "TaxOfficeId", "TaxNumber", "CareerAdvisorId", "Address", "DistrictId", "CountryId",
        "RejectionReason", "ApprovedByUserId", "ApprovedAtUtc", "DeactivatedByUserId", "DeactivatedAtUtc",
        "UserId", "Status", "MarketingConsent", "RowVersion",
    ];

    [Fact]
    public void ResponseType_DoesNotExposeForbiddenFields()
    {
        var propertyNames = typeof(GetPublicCompanyProfileResponse).GetProperties().Select(p => p.Name).ToHashSet();

        foreach (var forbidden in ForbiddenPropertyNames)
        {
            Assert.DoesNotContain(forbidden, propertyNames);
        }
    }
}
