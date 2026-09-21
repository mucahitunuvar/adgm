using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.RejectCompany;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.RejectCompany;

public class RejectCompanyCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private RejectCompanyCommandHandler CreateHandler() => new(_companyRepository, _unitOfWork);

    private static Company CreatePendingCompany() =>
        Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, null, DateTime.UtcNow);

    [Fact]
    public async Task Handle_WithPendingApprovalCompany_RejectsAndSaves()
    {
        var company = CreatePendingCompany();
        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(new RejectCompanyCommand(company.Id, "Eksik belge"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CompanyStatus.Rejected, company.Status);
        Assert.Equal("Eksik belge", company.RejectionReason);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound_AndDoesNotSave()
    {
        var result = await CreateHandler().Handle(new RejectCompanyCommand(Guid.NewGuid(), "Eksik belge"), CancellationToken.None);

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

        var result = await CreateHandler().Handle(new RejectCompanyCommand(company.Id, "Eksik belge"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
