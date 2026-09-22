using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsEmployer;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;
using GenclikMerkezi.UnitTests.Interview.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Interview.Features.RequestInterviewAsEmployer;

public class RequestInterviewAsEmployerCommandHandlerTests
{
    private readonly FakeInterviewRepository _interviewRepository = new();
    private readonly FakeCompanyModuleContract _companyModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly GenclikMerkezi.UnitTests.Candidate.TestDoubles.FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _employerUserId = Guid.NewGuid();

    private RequestInterviewAsEmployerCommandHandler CreateHandler() =>
        new(_interviewRepository, _companyModuleContract, _candidateModuleContract,
            new FakeCurrentUserContext(_employerUserId), _unitOfWork);

    [Fact]
    public async Task Handle_WithOwnCompanyAndCandidateHasAdvisor_CreatesInterview_AndSaves()
    {
        var companyId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        var advisorId = Guid.NewGuid();
        _companyModuleContract.Seed(new CompanySummary(companyId, "Acme A.Ş.", Guid.NewGuid(), _employerUserId, "firma@example.com"));
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, Guid.NewGuid(), "aday@example.com"));

        var result = await CreateHandler().Handle(new RequestInterviewAsEmployerCommand(candidateCvId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var interview = Assert.Single(_interviewRepository.Interviews);
        Assert.Equal(candidateCvId, interview.CandidateCvId);
        Assert.Equal(companyId, interview.CompanyId);
        Assert.Equal(advisorId, interview.OrganizingAdvisorId);
        Assert.Equal(interview.Id, result.Value.InterviewId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithNoCompanyForCurrentUser_ReturnsNotFound_AndDoesNotSave()
    {
        var result = await CreateHandler().Handle(new RequestInterviewAsEmployerCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithCandidateHavingNoAdvisor_ReturnsConflict_AndDoesNotSave()
    {
        var companyId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _companyModuleContract.Seed(new CompanySummary(companyId, "Acme A.Ş.", Guid.NewGuid(), _employerUserId, "firma@example.com"));
        // Aday bilinçli olarak seed edilmedi -> GetCareerAdvisorIdForCandidateAsync null döner.

        var result = await CreateHandler().Handle(new RequestInterviewAsEmployerCommand(candidateCvId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
