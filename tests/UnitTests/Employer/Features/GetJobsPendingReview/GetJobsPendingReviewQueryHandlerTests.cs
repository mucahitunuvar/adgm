using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.GetJobsPendingReview;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetJobsPendingReview;

public class GetJobsPendingReviewQueryHandlerTests
{
    private readonly FakeJobRepository _jobRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private GetJobsPendingReviewQueryHandler CreateHandler() =>
        new(_jobRepository, _careerAdvisorModuleContract, new FakeCurrentUserContext(_advisorUserId));

    private static Job CreateUnderReviewJob(Guid companyId)
    {
        var job = Job.Create(
            companyId, "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), null, Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        job.Submit();
        return job;
    }

    [Fact]
    public async Task Handle_ReturnsOnlyUnderReviewJobsAssignedToCaller()
    {
        var careerAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = careerAdvisorId;

        var myCompanyId = Guid.NewGuid();
        _jobRepository.RegisterCompanyCareerAdvisor(myCompanyId, careerAdvisorId);
        var myJob = CreateUnderReviewJob(myCompanyId);
        _jobRepository.Add(myJob);

        var otherCompanyId = Guid.NewGuid();
        _jobRepository.RegisterCompanyCareerAdvisor(otherCompanyId, Guid.NewGuid());
        _jobRepository.Add(CreateUnderReviewJob(otherCompanyId));

        var draftJob = Job.Create(
            myCompanyId, "Taslak", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), null, Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        _jobRepository.Add(draftJob);

        var result = await CreateHandler().Handle(new GetJobsPendingReviewQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var returnedJob = Assert.Single(result.Value);
        Assert.Equal(myJob.Id, returnedJob.Id);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotACareerAdvisor_ReturnsForbidden()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(new GetJobsPendingReviewQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }
}
