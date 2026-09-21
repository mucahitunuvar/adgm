using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.SubmitPersonnelNeed;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.SubmitPersonnelNeed;

public class SubmitPersonnelNeedCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakePersonnelNeedRepository _personnelNeedRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();
    private readonly GenclikMerkezi.UnitTests.Candidate.TestDoubles.FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _employerUserId = Guid.NewGuid();

    private SubmitPersonnelNeedCommandHandler CreateHandler() =>
        new(_companyRepository, _personnelNeedRepository, _careerAdvisorModuleContract, _notificationModuleContract,
            new FakeCurrentUserContext(_employerUserId), _unitOfWork);

    private Company CreateCompanyForCurrentUser(Guid? careerAdvisorId = null)
    {
        var company = Company.Create(
            _employerUserId, "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, careerAdvisorId, DateTime.UtcNow);
        _companyRepository.Add(company);
        return company;
    }

    private static PersonnelNeed CreateDraftPersonnelNeed(Guid companyId) =>
        PersonnelNeed.Create(
            companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid(),
            Guid.NewGuid(), null, [], [], [], [], DateTime.UtcNow);

    [Fact]
    public async Task Handle_WithOwnedTaslakPersonnelNeed_TransitionsToKendiHavuzunda_AndNotifiesAssignedAdvisor()
    {
        var advisorId = Guid.NewGuid();
        var advisorUserId = Guid.NewGuid();
        _careerAdvisorModuleContract.ActiveAdvisors = [new ActiveCareerAdvisorSummary(advisorId, advisorUserId, "danisman@example.com")];
        var company = CreateCompanyForCurrentUser(advisorId);
        var personnelNeed = CreateDraftPersonnelNeed(company.Id);
        _personnelNeedRepository.Add(personnelNeed);

        var result = await CreateHandler().Handle(new SubmitPersonnelNeedCommand(personnelNeed.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PersonnelNeedStatus.KendiHavuzunda, personnelNeed.Status);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);

        var notification = Assert.Single(_notificationModuleContract.SentNotifications);
        Assert.Equal(advisorUserId, notification.UserId);
        Assert.Equal("danisman@example.com", notification.RecipientEmail);
    }

    [Fact]
    public async Task Handle_WithNoAssignedAdvisor_Succeeds_AndDoesNotNotify()
    {
        var company = CreateCompanyForCurrentUser(careerAdvisorId: null);
        var personnelNeed = CreateDraftPersonnelNeed(company.Id);
        _personnelNeedRepository.Add(personnelNeed);

        var result = await CreateHandler().Handle(new SubmitPersonnelNeedCommand(personnelNeed.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_notificationModuleContract.SentNotifications);
    }

    [Fact]
    public async Task Handle_WithPersonnelNeedBelongingToAnotherCompany_ReturnsForbidden()
    {
        CreateCompanyForCurrentUser();
        var otherCompanyPersonnelNeed = CreateDraftPersonnelNeed(Guid.NewGuid());
        _personnelNeedRepository.Add(otherCompanyPersonnelNeed);

        var result = await CreateHandler().Handle(new SubmitPersonnelNeedCommand(otherCompanyPersonnelNeed.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithPersonnelNeedAlreadyInKendiHavuzunda_ReturnsConflict()
    {
        var company = CreateCompanyForCurrentUser();
        var personnelNeed = CreateDraftPersonnelNeed(company.Id);
        personnelNeed.Submit();
        _personnelNeedRepository.Add(personnelNeed);

        var result = await CreateHandler().Handle(new SubmitPersonnelNeedCommand(personnelNeed.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
