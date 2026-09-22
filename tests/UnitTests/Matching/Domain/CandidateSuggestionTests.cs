using GenclikMerkezi.Modules.Matching.Domain;

namespace GenclikMerkezi.UnitTests.Matching.Domain;

public class CandidateSuggestionTests
{
    private static CandidateSuggestion CreateSuggestion() =>
        CandidateSuggestion.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

    [Fact]
    public void Create_SetsAllFieldsAndDefaultsToOnerildi()
    {
        var personnelNeedId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        var suggestingAdvisorId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var suggestion = CandidateSuggestion.Create(personnelNeedId, candidateCvId, suggestingAdvisorId, createdAtUtc);

        Assert.Equal(CandidateSuggestionStatus.Onerildi, suggestion.Status);
        Assert.Equal(personnelNeedId, suggestion.PersonnelNeedId);
        Assert.Equal(candidateCvId, suggestion.CandidateCvId);
        Assert.Equal(suggestingAdvisorId, suggestion.SuggestingAdvisorId);
        Assert.Equal(createdAtUtc, suggestion.CreatedAtUtc);
        Assert.Null(suggestion.DecidedByAdvisorId);
        Assert.Null(suggestion.DecidedAtUtc);
    }

    [Fact]
    public void Accept_FromOnerildi_Succeeds()
    {
        var suggestion = CreateSuggestion();
        var decidedByAdvisorId = Guid.NewGuid();
        var decidedAtUtc = DateTime.UtcNow;

        var result = suggestion.Accept(decidedByAdvisorId, decidedAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(CandidateSuggestionStatus.KabulEdildi, suggestion.Status);
        Assert.Equal(decidedByAdvisorId, suggestion.DecidedByAdvisorId);
        Assert.Equal(decidedAtUtc, suggestion.DecidedAtUtc);
    }

    [Theory]
    [InlineData(CandidateSuggestionStatus.KabulEdildi)]
    [InlineData(CandidateSuggestionStatus.Reddedildi)]
    public void Accept_FromNonOnerildiStatus_Fails(CandidateSuggestionStatus initialStatus)
    {
        var suggestion = CreateSuggestion();
        TransitionTo(suggestion, initialStatus);

        var result = suggestion.Accept(Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, suggestion.Status);
    }

    [Fact]
    public void Reject_FromOnerildi_Succeeds()
    {
        var suggestion = CreateSuggestion();
        var decidedByAdvisorId = Guid.NewGuid();
        var decidedAtUtc = DateTime.UtcNow;

        var result = suggestion.Reject(decidedByAdvisorId, decidedAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(CandidateSuggestionStatus.Reddedildi, suggestion.Status);
        Assert.Equal(decidedByAdvisorId, suggestion.DecidedByAdvisorId);
        Assert.Equal(decidedAtUtc, suggestion.DecidedAtUtc);
    }

    [Theory]
    [InlineData(CandidateSuggestionStatus.KabulEdildi)]
    [InlineData(CandidateSuggestionStatus.Reddedildi)]
    public void Reject_FromNonOnerildiStatus_Fails(CandidateSuggestionStatus initialStatus)
    {
        var suggestion = CreateSuggestion();
        TransitionTo(suggestion, initialStatus);

        var result = suggestion.Reject(Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, suggestion.Status);
    }

    private static void TransitionTo(CandidateSuggestion suggestion, CandidateSuggestionStatus status)
    {
        if (status == CandidateSuggestionStatus.KabulEdildi)
        {
            suggestion.Accept(Guid.NewGuid(), DateTime.UtcNow);
            return;
        }

        suggestion.Reject(Guid.NewGuid(), DateTime.UtcNow);
    }
}
