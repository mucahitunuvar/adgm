using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.PoolPersonnelNeed;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.PoolPersonnelNeed;

public class PoolPersonnelNeedCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakePersonnelNeedRepository _personnelNeedRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();
    private readonly GenclikMerkezi.UnitTests.Candidate.TestDoubles.FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private PoolPersonnelNeedCommandHandler CreateHandler() =>
        new(_companyRepository, _personnelNeedRepository, _careerAdvisorModuleContract, _notificationModuleContract,
            new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    private (Company Company, PersonnelNeed PersonnelNeed) CreateKendiHavuzundaPersonnelNeedForAdvisor(Guid careerAdvisorId)
    {
        var company = Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, careerAdvisorId, DateTime.UtcNow);
        _companyRepository.Add(company);

        var personnelNeed = PersonnelNeed.Create(
            company.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid(),
            Guid.NewGuid(), null, [], [], [], [], DateTime.UtcNow);
        personnelNeed.Submit();
        _personnelNeedRepository.Add(personnelNeed);

        return (company, personnelNeed);
    }

    [Fact]
    public async Task Handle_AsAssignedAdvisor_PoolsPersonnelNeed_AndBroadcastsToOtherActiveAdvisors_ExcludingCaller()
    {
        var careerAdvisorId = Guid.NewGuid();
        var otherAdvisorId = Guid.NewGuid();
        var otherAdvisorUserId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = careerAdvisorId;
        _careerAdvisorModuleContract.ActiveAdvisors =
        [
            new ActiveCareerAdvisorSummary(careerAdvisorId, _advisorUserId, "caller@example.com"),
            new ActiveCareerAdvisorSummary(otherAdvisorId, otherAdvisorUserId, "other@example.com"),
        ];
        var (_, personnelNeed) = CreateKendiHavuzundaPersonnelNeedForAdvisor(careerAdvisorId);

        var result = await CreateHandler().Handle(new PoolPersonnelNeedCommand(personnelNeed.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PersonnelNeedStatus.GenelHavuzda, personnelNeed.Status);
        Assert.Equal(careerAdvisorId, personnelNeed.PooledByAdvisorId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);

        var broadcast = Assert.Single(_notificationModuleContract.SentBulkNotifications);
        var recipient = Assert.Single(broadcast.Recipients);
        Assert.Equal(otherAdvisorUserId, recipient.UserId);
    }

    [Fact]
    public async Task Handle_AsDifferentAdvisor_ReturnsForbidden_AndDoesNotSave()
    {
        var assignedAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        var (_, personnelNeed) = CreateKendiHavuzundaPersonnelNeedForAdvisor(assignedAdvisorId);

        var result = await CreateHandler().Handle(new PoolPersonnelNeedCommand(personnelNeed.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(PersonnelNeedStatus.KendiHavuzunda, personnelNeed.Status);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.Empty(_notificationModuleContract.SentBulkNotifications);
    }

    [Fact]
    public async Task Handle_WithUnknownPersonnelNeedId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new PoolPersonnelNeedCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithPersonnelNeedNotInKendiHavuzunda_ReturnsConflict()
    {
        var careerAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = careerAdvisorId;
        var (_, personnelNeed) = CreateKendiHavuzundaPersonnelNeedForAdvisor(careerAdvisorId);
        personnelNeed.PoolToGeneral(careerAdvisorId, DateTime.UtcNow);

        var result = await CreateHandler().Handle(new PoolPersonnelNeedCommand(personnelNeed.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
