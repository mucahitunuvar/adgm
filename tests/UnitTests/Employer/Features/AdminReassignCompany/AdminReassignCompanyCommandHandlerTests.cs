using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.AdminReassignCompany;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.AdminReassignCompany;

public class AdminReassignCompanyCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly GenclikMerkezi.UnitTests.Candidate.TestDoubles.FakeUnitOfWork _unitOfWork = new();

    private AdminReassignCompanyCommandHandler CreateHandler() =>
        new(_companyRepository, _careerAdvisorModuleContract, _unitOfWork);

    private static Company CreateCompany(Guid? careerAdvisorId = null) =>
        Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, careerAdvisorId, DateTime.UtcNow);

    [Fact]
    public async Task Handle_WithActiveAdvisor_ReassignsAndSaves()
    {
        var company = CreateCompany();
        _companyRepository.Add(company);
        var newAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.ActiveAdvisors = [new ActiveCareerAdvisorSummary(newAdvisorId, Guid.NewGuid(), "advisor@example.com")];

        var result = await CreateHandler().Handle(
            new AdminReassignCompanyCommand(company.Id, newAdvisorId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(newAdvisorId, company.CareerAdvisorId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithNull_RemovesAssignment_AndSaves()
    {
        var existingAdvisorId = Guid.NewGuid();
        var company = CreateCompany(existingAdvisorId);
        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(
            new AdminReassignCompanyCommand(company.Id, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(company.CareerAdvisorId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithInactiveOrUnknownAdvisor_ReturnsNotFound_AndDoesNotSave()
    {
        var company = CreateCompany();
        _companyRepository.Add(company);
        _careerAdvisorModuleContract.ActiveAdvisors = [];

        var result = await CreateHandler().Handle(
            new AdminReassignCompanyCommand(company.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Null(company.CareerAdvisorId);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownCompanyId_ReturnsNotFound_AndDoesNotSave()
    {
        var result = await CreateHandler().Handle(
            new AdminReassignCompanyCommand(Guid.NewGuid(), null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
