using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.ApproveCompany;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.ApproveCompany;

public class ApproveCompanyCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _adminUserId = Guid.NewGuid();

    private ApproveCompanyCommandHandler CreateHandler() =>
        new(_companyRepository, new FakeCurrentUserContext(_adminUserId), _unitOfWork);

    private static Company CreatePendingCompany()
    {
        var company = Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, null, DateTime.UtcNow);
        return company;
    }

    [Fact]
    public async Task Handle_WithPendingApprovalCompany_ApprovesAndSaves()
    {
        var company = CreatePendingCompany();
        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(new ApproveCompanyCommand(company.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CompanyStatus.Approved, company.Status);
        Assert.Equal(_adminUserId, company.ApprovedByUserId);
        Assert.NotNull(company.ApprovedAtUtc);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound_AndDoesNotSave()
    {
        var result = await CreateHandler().Handle(new ApproveCompanyCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithAlreadyApprovedCompany_ReturnsConflict_AndDoesNotSave()
    {
        var company = CreatePendingCompany();
        company.Approve(Guid.NewGuid(), DateTime.UtcNow);
        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(new ApproveCompanyCommand(company.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
