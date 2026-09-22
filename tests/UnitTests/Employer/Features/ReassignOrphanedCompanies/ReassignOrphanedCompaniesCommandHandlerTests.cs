using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.ReassignOrphanedCompanies;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.ReassignOrphanedCompanies;

public class ReassignOrphanedCompaniesCommandHandlerTests
{
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ReassignOrphanedCompaniesCommandHandler CreateHandler() =>
        new(_careerAdvisorModuleContract, _companyRepository, _unitOfWork);

    private static Company CreateOrphanedCompany(string name, Guid careerAdvisorId) =>
        Company.Create(
            Guid.NewGuid(), name, Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", $"{name}@example.com", "05550000000", Guid.NewGuid(),
            Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(), false, careerAdvisorId, DateTime.UtcNow);

    [Fact]
    public async Task Handle_WithNoOrphanedCompanies_ReturnsZero_AndDoesNotSave()
    {
        var deactivatedAdvisorId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new ReassignOrphanedCompaniesCommand(deactivatedAdvisorId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.ReassignedCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithOneActiveAdvisor_ReassignsAllOrphanedCompaniesToIt()
    {
        var deactivatedAdvisorId = Guid.NewGuid();
        var onlyActiveAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.ActiveAdvisors =
            [new ActiveCareerAdvisorSummary(onlyActiveAdvisorId, Guid.NewGuid(), "advisor@example.com")];

        var orphan1 = CreateOrphanedCompany("Acme", deactivatedAdvisorId);
        var orphan2 = CreateOrphanedCompany("Beta", deactivatedAdvisorId);
        _companyRepository.Add(orphan1);
        _companyRepository.Add(orphan2);

        var result = await CreateHandler().Handle(new ReassignOrphanedCompaniesCommand(deactivatedAdvisorId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.ReassignedCount);
        Assert.Equal(onlyActiveAdvisorId, orphan1.CareerAdvisorId);
        Assert.Equal(onlyActiveAdvisorId, orphan2.CareerAdvisorId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithNoActiveAdvisors_LeavesOrphanedCompaniesCareerAdvisorIdNull()
    {
        var deactivatedAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.ActiveAdvisors = [];

        var orphan = CreateOrphanedCompany("Acme", deactivatedAdvisorId);
        _companyRepository.Add(orphan);

        var result = await CreateHandler().Handle(new ReassignOrphanedCompaniesCommand(deactivatedAdvisorId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.ReassignedCount);
        Assert.Null(orphan.CareerAdvisorId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }
}
