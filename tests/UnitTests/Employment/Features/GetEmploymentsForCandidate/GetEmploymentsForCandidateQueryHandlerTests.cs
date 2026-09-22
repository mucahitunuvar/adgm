using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Employment.Features.GetEmploymentsForCandidate;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employment.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employment.Features.GetEmploymentsForCandidate;

public class GetEmploymentsForCandidateQueryHandlerTests
{
    private readonly FakeEmploymentRepository _employmentRepository = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly Guid _callerUserId = Guid.NewGuid();

    private GetEmploymentsForCandidateQueryHandler CreateHandler() =>
        new(_employmentRepository, _candidateModuleContract, _careerAdvisorModuleContract, new FakeCurrentUserContext(_callerUserId));

    [Fact]
    public async Task Handle_AsOwningCandidate_ReturnsOwnEmployments()
    {
        var candidateCvId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", Guid.NewGuid(), _callerUserId, "aday@example.com"));
        var employment = GenclikMerkezi.Modules.Employment.Domain.Employment.Create(
            candidateCvId, Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow, Guid.NewGuid(), DateTime.UtcNow);
        _employmentRepository.Add(employment);

        var result = await CreateHandler().Handle(new GetEmploymentsForCandidateQuery(candidateCvId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(employment.Id, Assert.Single(result.Value).Id);
    }

    [Fact]
    public async Task Handle_AsCurrentAdvisor_ReturnsCandidateEmployments()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, Guid.NewGuid(), "aday@example.com"));
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        var employment = GenclikMerkezi.Modules.Employment.Domain.Employment.Create(
            candidateCvId, Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow, advisorId, DateTime.UtcNow);
        _employmentRepository.Add(employment);

        var result = await CreateHandler().Handle(new GetEmploymentsForCandidateQuery(candidateCvId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(employment.Id, Assert.Single(result.Value).Id);
    }

    [Fact]
    public async Task Handle_AsUnrelatedCaller_ReturnsForbidden()
    {
        var candidateCvId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", Guid.NewGuid(), Guid.NewGuid(), "aday@example.com"));
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(new GetEmploymentsForCandidateQuery(candidateCvId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }

    [Fact]
    public async Task Handle_AsDifferentAdvisor_ReturnsForbidden()
    {
        var candidateCvId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", Guid.NewGuid(), Guid.NewGuid(), "aday@example.com"));
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();

        var result = await CreateHandler().Handle(new GetEmploymentsForCandidateQuery(candidateCvId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }
}
