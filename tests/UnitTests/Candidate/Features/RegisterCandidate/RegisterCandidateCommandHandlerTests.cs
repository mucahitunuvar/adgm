using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;

namespace GenclikMerkezi.UnitTests.Candidate.Features.RegisterCandidate;

public class RegisterCandidateCommandHandlerTests
{
    private readonly FakeIdentityService _identityService = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateCvRepository _candidateCvRepository = new();
    private readonly FakeCandidateCvContentRepository _candidateCvContentRepository = new();
    private readonly FakeCandidateSearchIndexRepository _candidateSearchIndexRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private RegisterCandidateCommandHandler CreateHandler() =>
        new(_identityService, _careerAdvisorModuleContract, _candidateCvRepository, _candidateCvContentRepository, _candidateSearchIndexRepository, _unitOfWork);

    private static RegisterCandidateCommand ValidCommand() =>
        new("aday@example.com", "Sifre123", "Ahmet", "Yılmaz", "05551234567");

    [Fact]
    public async Task Handle_WithValidInput_CreatesUserThenCandidateCvAndContent()
    {
        var userId = Guid.NewGuid();
        _identityService.CreateUserResult = Result.Success(userId);

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.UserId);

        var candidateCv = Assert.Single(_candidateCvRepository.CandidateCvs);
        Assert.Equal(userId, candidateCv.UserId);
        Assert.Equal("Ahmet", candidateCv.FirstName);
        Assert.Equal("Yılmaz", candidateCv.LastName);
        Assert.Equal("aday@example.com", candidateCv.Email);
        Assert.Equal("05551234567", candidateCv.PhoneNumber);
        Assert.Equal(candidateCv.Id, result.Value.CandidateCvId);

        var candidateCvContent = Assert.Single(_candidateCvContentRepository.CandidateCvContents);
        Assert.Equal(candidateCv.Id, candidateCvContent.CandidateCvId);

        var searchIndex = Assert.Single(_candidateSearchIndexRepository.SearchIndexes);
        Assert.Equal(candidateCv.Id, searchIndex.Id);
        Assert.Equal("AHMET YILMAZ", searchIndex.FullNameNormalized);
        Assert.Equal("aday@example.com", searchIndex.Email);

        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.False(_identityService.DeactivateUserAsyncCalled);
        Assert.Null(candidateCv.CareerAdvisorId);
    }

    [Fact]
    public async Task Handle_WithNoActiveCareerAdvisors_RegistersSuccessfully_AndLeavesCareerAdvisorIdNull()
    {
        _identityService.CreateUserResult = Result.Success(Guid.NewGuid());
        _careerAdvisorModuleContract.ActiveAdvisors = [];

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var candidateCv = Assert.Single(_candidateCvRepository.CandidateCvs);
        Assert.Null(candidateCv.CareerAdvisorId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithActiveCareerAdvisors_AssignsTheLeastLoadedOne()
    {
        _identityService.CreateUserResult = Result.Success(Guid.NewGuid());
        var busyAdvisorId = Guid.NewGuid();
        var idleAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.ActiveAdvisors =
        [
            new ActiveCareerAdvisorSummary(busyAdvisorId, Guid.NewGuid(), "advisor-busy@example.com"),
            new ActiveCareerAdvisorSummary(idleAdvisorId, Guid.NewGuid(), "advisor-idle@example.com"),
        ];

        // busyAdvisorId already has two existing candidates, idleAdvisorId has none.
        _candidateCvRepository.Add(GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            Guid.NewGuid(), "A", "A", "a1@example.com", null, busyAdvisorId));
        _candidateCvRepository.Add(GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            Guid.NewGuid(), "B", "B", "a2@example.com", null, busyAdvisorId));

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var newCandidateCv = _candidateCvRepository.CandidateCvs.Single(c => c.Email == "aday@example.com");
        Assert.Equal(idleAdvisorId, newCandidateCv.CareerAdvisorId);
    }

    [Fact]
    public async Task Handle_WhenIdentityServiceReportsEmailAlreadyExists_ReturnsFailure_AndDoesNotCreateCandidateCv()
    {
        var error = Error.Conflict("User.EmailAlreadyExists", "A user with this email already exists.");
        _identityService.CreateUserResult = Result.Failure<Guid>(error);

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Empty(_candidateCvRepository.CandidateCvs);
        Assert.Empty(_candidateCvContentRepository.CandidateCvContents);
        Assert.Empty(_candidateSearchIndexRepository.SearchIndexes);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.False(_identityService.DeactivateUserAsyncCalled);
    }

    [Fact]
    public async Task Handle_WhenCandidateCvPersistenceFails_DeactivatesUser_AndRethrows()
    {
        var userId = Guid.NewGuid();
        _identityService.CreateUserResult = Result.Success(userId);
        _unitOfWork.ThrowOnSave = new InvalidOperationException("Simulated CandidateCv persistence failure.");

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(ValidCommand(), CancellationToken.None));

        Assert.True(_identityService.DeactivateUserAsyncCalled);
        Assert.Equal(userId, _identityService.DeactivatedUserId);
    }
}
