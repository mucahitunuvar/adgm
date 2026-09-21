using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.SubmitJobForReview;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.SubmitJobForReview;

public class SubmitJobForReviewCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeJobRepository _jobRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _employerUserId = Guid.NewGuid();

    private SubmitJobForReviewCommandHandler CreateHandler() =>
        new(_companyRepository, _jobRepository, new FakeCurrentUserContext(_employerUserId), _unitOfWork);

    private Company CreateCompanyForCurrentUser()
    {
        var company = Company.Create(
            _employerUserId, "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, null, DateTime.UtcNow);
        _companyRepository.Add(company);
        return company;
    }

    private static Job CreateDraftJob(Guid companyId) =>
        Job.Create(
            companyId, "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), null, Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);

    [Fact]
    public async Task Handle_WithOwnedDraftJob_TransitionsToUnderReview_AndSaves()
    {
        var company = CreateCompanyForCurrentUser();
        var job = CreateDraftJob(company.Id);
        _jobRepository.Add(job);

        var result = await CreateHandler().Handle(new SubmitJobForReviewCommand(job.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(JobStatus.UnderReview, job.Status);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithJobBelongingToAnotherCompany_ReturnsForbidden()
    {
        CreateCompanyForCurrentUser();
        var otherCompanyJob = CreateDraftJob(Guid.NewGuid());
        _jobRepository.Add(otherCompanyJob);

        var result = await CreateHandler().Handle(new SubmitJobForReviewCommand(otherCompanyJob.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithJobAlreadyUnderReview_ReturnsConflict()
    {
        var company = CreateCompanyForCurrentUser();
        var job = CreateDraftJob(company.Id);
        job.Submit();
        _jobRepository.Add(job);

        var result = await CreateHandler().Handle(new SubmitJobForReviewCommand(job.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
