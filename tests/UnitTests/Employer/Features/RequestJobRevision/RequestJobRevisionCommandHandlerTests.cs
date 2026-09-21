using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.RequestJobRevision;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.RequestJobRevision;

public class RequestJobRevisionCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeJobRepository _jobRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();
    private readonly GenclikMerkezi.UnitTests.Candidate.TestDoubles.FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private RequestJobRevisionCommandHandler CreateHandler() =>
        new(_companyRepository, _jobRepository, _careerAdvisorModuleContract, _notificationModuleContract,
            new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    private (Company Company, Job Job) CreateUnderReviewJobForAdvisor(Guid careerAdvisorId)
    {
        var company = Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, careerAdvisorId, DateTime.UtcNow);
        _companyRepository.Add(company);

        var job = Job.Create(
            company.Id, "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), null, Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        job.Submit();
        _jobRepository.Add(job);

        return (company, job);
    }

    [Fact]
    public async Task Handle_AsAssignedAdvisor_RequestsRevision_AndNotifiesCompanyWithNotes()
    {
        var careerAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = careerAdvisorId;
        var (company, job) = CreateUnderReviewJobForAdvisor(careerAdvisorId);

        var result = await CreateHandler().Handle(
            new RequestJobRevisionCommand(job.Id, "Lütfen açıklamayı detaylandırın"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(JobStatus.RevisionRequested, job.Status);
        Assert.Equal("Lütfen açıklamayı detaylandırın", job.RevisionNotes);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);

        var notification = Assert.Single(_notificationModuleContract.SentNotifications);
        Assert.Equal(company.UserId, notification.UserId);
        Assert.Contains("Lütfen açıklamayı detaylandırın", notification.Message);
    }

    [Fact]
    public async Task Handle_AsDifferentAdvisor_ReturnsForbidden_AndDoesNotSave()
    {
        var assignedAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        var (_, job) = CreateUnderReviewJobForAdvisor(assignedAdvisorId);

        var result = await CreateHandler().Handle(new RequestJobRevisionCommand(job.Id, "notlar"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.Empty(_notificationModuleContract.SentNotifications);
    }

    [Fact]
    public async Task Handle_WithUnknownJobId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new RequestJobRevisionCommand(Guid.NewGuid(), "notlar"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
