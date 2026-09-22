using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Employment.Domain;
using GenclikMerkezi.Modules.Employment.Features.CreateEmployment;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employment.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employment.Features.CreateEmployment;

public class CreateEmploymentCommandHandlerTests
{
    private readonly FakeEmploymentRepository _employmentRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly GenclikMerkezi.UnitTests.Candidate.TestDoubles.FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private CreateEmploymentCommandHandler CreateHandler() =>
        new(_employmentRepository, _careerAdvisorModuleContract, _candidateModuleContract,
            new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    [Fact]
    public async Task Handle_WithOwnCandidate_CreatesEmployment_AndSaves()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var positionId = Guid.NewGuid();
        var interviewId = Guid.NewGuid();
        var startDateUtc = DateTime.UtcNow;
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, Guid.NewGuid(), "aday@example.com"));

        var result = await CreateHandler().Handle(
            new CreateEmploymentCommand(candidateCvId, companyId, positionId, interviewId, startDateUtc), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var employment = Assert.Single(_employmentRepository.Employments);
        Assert.Equal(candidateCvId, employment.CandidateCvId);
        Assert.Equal(companyId, employment.CompanyId);
        Assert.Equal(positionId, employment.PositionId);
        Assert.Equal(interviewId, employment.InterviewId);
        Assert.Equal(startDateUtc, employment.StartDateUtc);
        Assert.Equal(advisorId, employment.CreatedByAdvisorId);
        Assert.Equal(EmploymentStatus.DevamEdiyor, employment.Status);
        Assert.Equal(employment.Id, result.Value.EmploymentId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithoutInterviewId_CreatesEmployment_WithNullInterviewId()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, Guid.NewGuid(), "aday@example.com"));

        var result = await CreateHandler().Handle(
            new CreateEmploymentCommand(candidateCvId, Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(_employmentRepository.Employments).InterviewId);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotACareerAdvisor_ReturnsForbidden()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(
            new CreateEmploymentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_employmentRepository.Employments);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithCandidateNotOwnedByCaller_ReturnsForbidden()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", Guid.NewGuid(), Guid.NewGuid(), "aday@example.com"));

        var result = await CreateHandler().Handle(
            new CreateEmploymentCommand(candidateCvId, Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_employmentRepository.Employments);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
