using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.UpdatePersonnelNeed;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.UpdatePersonnelNeed;

public class UpdatePersonnelNeedCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakePersonnelNeedRepository _personnelNeedRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _employerUserId = Guid.NewGuid();

    private UpdatePersonnelNeedCommandHandler CreateHandler() =>
        new(_companyRepository, _personnelNeedRepository, new FakeCurrentUserContext(_employerUserId), _unitOfWork);

    private static UpdatePersonnelNeedCommand ValidCommand(Guid personnelNeedId) =>
        new(personnelNeedId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, Guid.NewGuid(),
            Guid.NewGuid(), "Güncellendi", [], [], [], []);

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

    private static PersonnelNeed CreateDraftPersonnelNeed(Guid companyId) =>
        PersonnelNeed.Create(
            companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid(),
            Guid.NewGuid(), null, [], [], [], [], DateTime.UtcNow);

    [Fact]
    public async Task Handle_WithOwnedTaslakPersonnelNeed_UpdatesAndSaves()
    {
        var company = CreateApprovedCompanyForCurrentUser();
        var personnelNeed = CreateDraftPersonnelNeed(company.Id);
        _personnelNeedRepository.Add(personnelNeed);

        var result = await CreateHandler().Handle(ValidCommand(personnelNeed.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, personnelNeed.Quantity);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithNoCompanyForCurrentUser_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownPersonnelNeedId_ReturnsNotFound()
    {
        CreateApprovedCompanyForCurrentUser();

        var result = await CreateHandler().Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithPersonnelNeedBelongingToAnotherCompany_ReturnsForbidden()
    {
        CreateApprovedCompanyForCurrentUser();
        var otherCompanyPersonnelNeed = CreateDraftPersonnelNeed(Guid.NewGuid());
        _personnelNeedRepository.Add(otherCompanyPersonnelNeed);

        var result = await CreateHandler().Handle(ValidCommand(otherCompanyPersonnelNeed.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithPersonnelNeedNotInTaslak_ReturnsConflict()
    {
        var company = CreateApprovedCompanyForCurrentUser();
        var personnelNeed = CreateDraftPersonnelNeed(company.Id);
        personnelNeed.Submit();
        _personnelNeedRepository.Add(personnelNeed);

        var result = await CreateHandler().Handle(ValidCommand(personnelNeed.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
