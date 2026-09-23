using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.Domain;

public class AdvisorRecommendationTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var candidateCvId = Guid.NewGuid();
        var advisorId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var recommendation = AdvisorRecommendation.Create(candidateCvId, advisorId, "Networking etkinliklerine katılmalı", createdAtUtc);

        Assert.Equal(candidateCvId, recommendation.CandidateCvId);
        Assert.Equal(advisorId, recommendation.AdvisorId);
        Assert.Equal("Networking etkinliklerine katılmalı", recommendation.Content);
        Assert.Equal(createdAtUtc, recommendation.CreatedAtUtc);
    }
}
