using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.DeactivateCompany;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.DeactivateCompany;

public class DeactivateCompanyCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _adminUserId = Guid.NewGuid();

    private DeactivateCompanyCommandHandler CreateHandler() =>
        new(_companyRepository, new FakeCurrentUserContext(_adminUserId), _unitOfWork);

    private static Company CreatePendingCompany() =>
        Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, null, DateTime.UtcNow);

    [Fact]
    public async Task Handle_WithApprovedCompany_DeactivatesAndSaves()
    {
        var company = CreatePendingCompany();
        company.Approve(Guid.NewGuid(), DateTime.UtcNow);
        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(new DeactivateCompanyCommand(company.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CompanyStatus.Deactivated, company.Status);
        Assert.Equal(_adminUserId, company.DeactivatedByUserId);
        Assert.NotNull(company.DeactivatedAtUtc);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound_AndDoesNotSave()
    {
        var result = await CreateHandler().Handle(new DeactivateCompanyCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithPendingApprovalCompany_ReturnsConflict_AndDoesNotSave()
    {
        var company = CreatePendingCompany();
        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(new DeactivateCompanyCommand(company.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithAlreadyDeactivatedCompany_ReturnsConflict_AndDoesNotSave()
    {
        var company = CreatePendingCompany();
        company.Approve(Guid.NewGuid(), DateTime.UtcNow);
        company.Deactivate(Guid.NewGuid(), DateTime.UtcNow);
        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(new DeactivateCompanyCommand(company.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
