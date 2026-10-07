using GenclikMerkezi.Modules.Employer.Features.GetPublicJobDetail;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetPublicJobDetail;

// Görev 3 (Employer public jobs master prompt) Testler: cinsiyet tercihi hiçbir yanıtta yok;
// danışman/inceleme/ret/revizyon alanları yok. Askerlik durumu tercihi burada DA VAR (yalnızca
// detayda - kullanıcı kararı), bu yüzden GetPublicJobs'un forbidden listesinden farklı.
public class GetPublicJobDetailResponseTests
{
    private static readonly string[] ForbiddenPropertyNames =
    [
        "GenderPreferenceIds", "GenderPreferences",
        "ReviewedByAdvisorId", "ReviewedAtUtc", "RejectionReason", "RevisionNotes",
        "SuspendedByUserId", "SuspendedAtUtc", "SuspensionReason", "CreatedAtUtc",
    ];

    [Fact]
    public void ResponseType_DoesNotExposeForbiddenFields()
    {
        var propertyNames = typeof(GetPublicJobDetailResponse).GetProperties().Select(p => p.Name).ToHashSet();

        foreach (var forbidden in ForbiddenPropertyNames)
        {
            Assert.DoesNotContain(forbidden, propertyNames);
        }
    }
}
