using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.CreatePersonnelNeed;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.CreatePersonnelNeed;

public class CreatePersonnelNeedCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakePersonnelNeedRepository _personnelNeedRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _employerUserId = Guid.NewGuid();

    private CreatePersonnelNeedCommandHandler CreateHandler() =>
        new(_companyRepository, _personnelNeedRepository, new FakeCurrentUserContext(_employerUserId), _unitOfWork);

    private static CreatePersonnelNeedCommand ValidCommand() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid(), Guid.NewGuid(), "Acil", [], [], [], []);

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
    public async Task Handle_WithApprovedCompany_CreatesPersonnelNeedInTaslak_AndSaves()
    {
        var company = CreateApprovedCompanyForCurrentUser();

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var personnelNeed = Assert.Single(_personnelNeedRepository.PersonnelNeeds);
        Assert.Equal(company.Id, personnelNeed.CompanyId);
        Assert.Equal(PersonnelNeedStatus.Taslak, personnelNeed.Status);
        Assert.Equal(personnelNeed.Id, result.Value.PersonnelNeedId);
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
        Assert.Empty(_personnelNeedRepository.PersonnelNeeds);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
