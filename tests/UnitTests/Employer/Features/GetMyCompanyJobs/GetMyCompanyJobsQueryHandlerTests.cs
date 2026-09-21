using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.GetMyCompanyJobs;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetMyCompanyJobs;

public class GetMyCompanyJobsQueryHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeJobRepository _jobRepository = new();
    private readonly Guid _employerUserId = Guid.NewGuid();

    private GetMyCompanyJobsQueryHandler CreateHandler() =>
        new(_companyRepository, _jobRepository, new FakeCurrentUserContext(_employerUserId));

    private static Job CreateJob(Guid companyId) =>
        Job.Create(
            companyId, "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), null, Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);

    [Fact]
    public async Task Handle_ReturnsOnlyJobsBelongingToCurrentUsersCompany()
    {
        var company = Company.Create(
            _employerUserId, "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, null, DateTime.UtcNow);
        _companyRepository.Add(company);

        var ownJob = CreateJob(company.Id);
        _jobRepository.Add(ownJob);

        var otherCompanyJob = CreateJob(Guid.NewGuid());
        _jobRepository.Add(otherCompanyJob);

        var result = await CreateHandler().Handle(new GetMyCompanyJobsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var returnedJob = Assert.Single(result.Value);
        Assert.Equal(ownJob.Id, returnedJob.Id);
    }

    [Fact]
    public async Task Handle_WithNoCompanyForCurrentUser_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new GetMyCompanyJobsQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
