using GenclikMerkezi.Modules.Candidate.Features.SendBulkCandidateNotification;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;

namespace GenclikMerkezi.UnitTests.Candidate.Features.SendBulkCandidateNotification;

public class SendBulkCandidateNotificationCommandHandlerTests
{
    private readonly FakeCandidateCvRepository _candidateCvRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();

    private SendBulkCandidateNotificationCommandHandler CreateHandler(Guid? currentUserId) =>
        new(_candidateCvRepository, new FakeCurrentUserContext(currentUserId), _careerAdvisorModuleContract);

    private GenclikMerkezi.Modules.Candidate.Domain.CandidateCv SeedCandidate(Guid careerAdvisorId, string email = "aday@example.com") =>
        GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            Guid.NewGuid(), "Ahmet", "Yılmaz", email, null, careerAdvisorId);

    [Fact]
    public async Task Handle_WhenNotAnActiveAdvisor_ReturnsForbidden()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new SendBulkCandidateNotificationCommand(null, "Duyuru", "Merhaba"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Null(_careerAdvisorModuleContract.SendBulkNotificationCall);
    }

    [Fact]
    public async Task Handle_WhenAdvisorHasNoCandidates_ReturnsConflict()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();

        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new SendBulkCandidateNotificationCommand(null, "Duyuru", "Merhaba"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithNoExplicitCandidateCvIds_TargetsAllOwnCandidates()
    {
        var advisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        var candidate1 = SeedCandidate(advisorId);
        var candidate2 = SeedCandidate(advisorId);
        _candidateCvRepository.Add(candidate1);
        _candidateCvRepository.Add(candidate2);
        // Başka bir danışmana ait aday - sonuca dahil edilmemeli.
        _candidateCvRepository.Add(SeedCandidate(Guid.NewGuid()));

        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new SendBulkCandidateNotificationCommand(null, "Duyuru", "Merhaba"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.RecipientCount);
        var call = _careerAdvisorModuleContract.SendBulkNotificationCall!.Value;
        Assert.Equal(2, call.CandidateUserIds.Count);
        Assert.Contains(candidate1.UserId, call.CandidateUserIds);
        Assert.Contains(candidate2.UserId, call.CandidateUserIds);
        Assert.Equal("Duyuru", call.Subject);
        Assert.Equal("Merhaba", call.Message);
    }

    [Fact]
    public async Task Handle_WithExplicitCandidateCvIds_ExcludesNonOwnedIds()
    {
        var advisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        var ownCandidate = SeedCandidate(advisorId);
        var otherAdvisorCandidate = SeedCandidate(Guid.NewGuid());
        _candidateCvRepository.Add(ownCandidate);
        _candidateCvRepository.Add(otherAdvisorCandidate);

        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new SendBulkCandidateNotificationCommand(
                [ownCandidate.Id, otherAdvisorCandidate.Id, Guid.NewGuid()], "Duyuru", "Merhaba"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.RecipientCount);
        var call = _careerAdvisorModuleContract.SendBulkNotificationCall!.Value;
        Assert.Equal([ownCandidate.UserId], call.CandidateUserIds);
    }
}
