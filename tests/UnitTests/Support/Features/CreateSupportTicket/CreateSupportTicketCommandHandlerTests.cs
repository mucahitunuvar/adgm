using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.Modules.Support.Features.CreateSupportTicket;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Support.TestDoubles;

namespace GenclikMerkezi.UnitTests.Support.Features.CreateSupportTicket;

public class CreateSupportTicketCommandHandlerTests
{
    private readonly FakeSupportTicketRepository _supportTicketRepository = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeCompanyModuleContract _companyModuleContract = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeIdentityService _identityService = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateSupportTicketCommandHandler CreateHandler(Guid callerUserId) =>
        new(
            _supportTicketRepository,
            _candidateModuleContract,
            _companyModuleContract,
            _careerAdvisorModuleContract,
            _identityService,
            _notificationModuleContract,
            new FakeCurrentUserContext(callerUserId),
            _unitOfWork);

    [Fact]
    public async Task Handle_AsCandidateWithActiveAdvisor_CreatesTicket_AssignedToAdvisorUserId()
    {
        var candidateUserId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        var advisorId = Guid.NewGuid();
        var advisorUserId = Guid.NewGuid();
        _candidateModuleContract.Seed(
            new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, candidateUserId, "aday@example.com"));
        _careerAdvisorModuleContract.ActiveAdvisors = [new ActiveCareerAdvisorSummary(advisorId, advisorUserId, "danisman@example.com")];

        var result = await CreateHandler(candidateUserId).Handle(
            new CreateSupportTicketCommand("Şifremi unuttum", nameof(SupportTicketPriority.Orta)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var ticket = Assert.Single(_supportTicketRepository.Tickets);
        Assert.Equal(SupportTicketOpenerRole.Candidate, ticket.OpenedByRole);
        Assert.Equal(candidateUserId, ticket.OpenedByUserId);
        Assert.Equal(candidateCvId, ticket.CandidateCvId);
        Assert.Null(ticket.CompanyId);
        Assert.Equal(advisorUserId, ticket.AssignedToUserId);
        Assert.Equal(SupportTicketStatus.Acik, ticket.Status);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_AsCandidateWithoutAnAdvisor_CreatesTicket_WithNullAssignee()
    {
        var candidateUserId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _candidateModuleContract.Seed(
            new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", null, candidateUserId, "aday@example.com"));

        var result = await CreateHandler(candidateUserId).Handle(
            new CreateSupportTicketCommand("Konu", nameof(SupportTicketPriority.Dusuk)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(_supportTicketRepository.Tickets).AssignedToUserId);
    }

    [Fact]
    public async Task Handle_AsCandidateWhoseAdvisorIsNoLongerActive_CreatesTicket_WithNullAssignee()
    {
        var candidateUserId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        var advisorId = Guid.NewGuid();
        _candidateModuleContract.Seed(
            new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, candidateUserId, "aday@example.com"));
        _careerAdvisorModuleContract.ActiveAdvisors = [];

        var result = await CreateHandler(candidateUserId).Handle(
            new CreateSupportTicketCommand("Konu", nameof(SupportTicketPriority.Dusuk)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(_supportTicketRepository.Tickets).AssignedToUserId);
    }

    [Fact]
    public async Task Handle_AsEmployer_CreatesTicket_WithEmployerRole()
    {
        var employerUserId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var advisorId = Guid.NewGuid();
        var advisorUserId = Guid.NewGuid();
        _companyModuleContract.Seed(new CompanySummary(companyId, "Acme", advisorId, employerUserId, "acme@example.com"));
        _careerAdvisorModuleContract.ActiveAdvisors = [new ActiveCareerAdvisorSummary(advisorId, advisorUserId, "danisman@example.com")];

        var result = await CreateHandler(employerUserId).Handle(
            new CreateSupportTicketCommand("Fatura sorunu", nameof(SupportTicketPriority.Kritik)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var ticket = Assert.Single(_supportTicketRepository.Tickets);
        Assert.Equal(SupportTicketOpenerRole.Employer, ticket.OpenedByRole);
        Assert.Equal(companyId, ticket.CompanyId);
        Assert.Null(ticket.CandidateCvId);
        Assert.Equal(advisorUserId, ticket.AssignedToUserId);
    }

    [Fact]
    public async Task Handle_WhenCallerHasNoCandidateOrCompanyProfile_ReturnsForbidden()
    {
        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new CreateSupportTicketCommand("Konu", nameof(SupportTicketPriority.Orta)), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_supportTicketRepository.Tickets);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_OnSuccess_NotifiesOpener()
    {
        var candidateUserId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _candidateModuleContract.Seed(
            new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", null, candidateUserId, "aday@example.com"));
        _identityService.UserProfileResult = new IdentityUserProfile(candidateUserId, "aday@example.com", "Ahmet", "Yılmaz", null);

        var result = await CreateHandler(candidateUserId).Handle(
            new CreateSupportTicketCommand("Konu", nameof(SupportTicketPriority.Orta)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var notification = Assert.Single(_notificationModuleContract.SentNotifications);
        Assert.Equal(candidateUserId, notification.UserId);
        Assert.Equal("aday@example.com", notification.RecipientEmail);
    }
}
