using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.CreateJob;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.CreateJob;

public class CreateJobCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeJobRepository _jobRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _employerUserId = Guid.NewGuid();

    private CreateJobCommandHandler CreateHandler() =>
        new(_companyRepository, _jobRepository, new FakeCurrentUserContext(_employerUserId), _unitOfWork);

    private static CreateJobCommand ValidCommand() =>
        new(
            "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], []);

    private Company CreateApprovedCompanyForCurrentUser()
    {
        var company = Company.Create(
            _employerUserId, "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, null, DateTime.UtcNow);
        company.Approve(Guid.NewGuid(), DateTime.UtcNow);
        _companyRepository.Add(company);
        return company;
    }

    [Fact]
    public async Task Handle_WithApprovedCompany_CreatesJobInDraft_AndSaves()
    {
        var company = CreateApprovedCompanyForCurrentUser();

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var job = Assert.Single(_jobRepository.Jobs);
        Assert.Equal(company.Id, job.CompanyId);
        Assert.Equal(JobStatus.Draft, job.Status);
        Assert.Equal(job.Id, result.Value.JobId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithNoCompanyForCurrentUser_ReturnsNotFound_AndDoesNotSave()
    {
        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithPendingApprovalCompany_ReturnsConflict_AndDoesNotSave()
    {
        var company = Company.Create(
            _employerUserId, "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, null, DateTime.UtcNow);
        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Empty(_jobRepository.Jobs);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
