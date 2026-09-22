using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Interview.Domain;
using GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsEmployer;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;
using GenclikMerkezi.UnitTests.Interview.TestDoubles;

namespace GenclikMerkezi.UnitTests.Interview.Features.GetMyInterviewsAsEmployer;

public class GetMyInterviewsAsEmployerQueryHandlerTests
{
    private readonly FakeInterviewRepository _interviewRepository = new();
    private readonly FakeCompanyModuleContract _companyModuleContract = new();
    private readonly Guid _employerUserId = Guid.NewGuid();

    private GetMyInterviewsAsEmployerQueryHandler CreateHandler() =>
        new(_interviewRepository, _companyModuleContract, new FakeCurrentUserContext(_employerUserId));

    [Fact]
    public async Task Handle_ReturnsOwnCompanysInterviews()
    {
        var companyId = Guid.NewGuid();
        _companyModuleContract.Seed(new CompanySummary(companyId, "Acme A.Ş.", Guid.NewGuid(), _employerUserId, "firma@example.com"));

        var ownInterview = GenclikMerkezi.Modules.Interview.Domain.Interview.Create(
            Guid.NewGuid(), companyId, Guid.NewGuid(), InterviewRequestedByRole.Employer, DateTime.UtcNow);
        _interviewRepository.Add(ownInterview);

        var otherInterview = GenclikMerkezi.Modules.Interview.Domain.Interview.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), InterviewRequestedByRole.Employer, DateTime.UtcNow);
        _interviewRepository.Add(otherInterview);

        var result = await CreateHandler().Handle(new GetMyInterviewsAsEmployerQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var returned = Assert.Single(result.Value);
        Assert.Equal(ownInterview.Id, returned.Id);
    }

    [Fact]
    public async Task Handle_WithNoCompanyForCurrentUser_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new GetMyInterviewsAsEmployerQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
