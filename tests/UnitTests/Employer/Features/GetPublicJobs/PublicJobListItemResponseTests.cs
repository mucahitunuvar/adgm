using GenclikMerkezi.Modules.Employer.Features.GetPublicJobs;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetPublicJobs;

// Görev 3 (Employer public jobs master prompt) Testler: "yanıtta yasaklı alanların bulunmaması" -
// cinsiyet tercihi hiçbir yanıtta yok, askerlik durumu tercihi yalnızca detayda var (listede yok).
public class PublicJobListItemResponseTests
{
    private static readonly string[] ForbiddenPropertyNames =
    [
        "GenderPreferenceIds", "GenderPreferences", "MilitaryStatusPreferenceIds", "MilitaryStatusPreferenceNames",
        "ReviewedByAdvisorId", "ReviewedAtUtc", "RejectionReason", "RevisionNotes", "SuspendedByUserId",
        "SuspendedAtUtc", "SuspensionReason", "DescriptionHtml", "CompanyId",
    ];

    [Fact]
    public void ResponseType_DoesNotExposeForbiddenFields()
    {
        var propertyNames = typeof(PublicJobListItemResponse).GetProperties().Select(p => p.Name).ToHashSet();

        foreach (var forbidden in ForbiddenPropertyNames)
        {
            Assert.DoesNotContain(forbidden, propertyNames);
        }
    }
}
