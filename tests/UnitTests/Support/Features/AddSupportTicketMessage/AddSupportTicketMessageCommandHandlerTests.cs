using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.Modules.Support.Features.AddSupportTicketMessage;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Support.TestDoubles;

namespace GenclikMerkezi.UnitTests.Support.Features.AddSupportTicketMessage;

public class AddSupportTicketMessageCommandHandlerTests
{
    private readonly FakeSupportTicketRepository _supportTicketRepository = new();
    private readonly FakeSupportTicketMessageRepository _supportTicketMessageRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeIdentityService _identityService = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private AddSupportTicketMessageCommandHandler CreateHandler(Guid callerUserId) =>
        new(
            _supportTicketRepository,
            _supportTicketMessageRepository,
            _careerAdvisorModuleContract,
            _identityService,
            _notificationModuleContract,
            new FakeCurrentUserContext(callerUserId),
            _unitOfWork);

    private static SupportTicket SeedTicket(
        FakeSupportTicketRepository repository, Guid openedByUserId, Guid? assignedToUserId, SupportTicketStatus status = SupportTicketStatus.Acik)
    {
        var ticket = SupportTicket.Create(
            SupportTicketOpenerRole.Candidate, openedByUserId, Guid.NewGuid(), null,
            "Konu", SupportTicketPriority.Orta, assignedToUserId, DateTime.UtcNow);

        if (status == SupportTicketStatus.Cevaplandi)
        {
            ticket.MarkAnswered(DateTime.UtcNow);
        }
        else if (status == SupportTicketStatus.Kapandi)
        {
            ticket.Close(assignedToUserId, "Kapatıldı", DateTime.UtcNow);
        }

        repository.Add(ticket);
        return ticket;
    }

    [Fact]
    public async Task Handle_WhenCallerIsNeitherOpenerNorAssignee_ReturnsForbidden()
    {
        var ticket = SeedTicket(_supportTicketRepository, Guid.NewGuid(), Guid.NewGuid());

        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new AddSupportTicketMessageCommand(ticket.Id, "Merhaba"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_supportTicketMessageRepository.Messages);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownTicketId_ReturnsNotFound()
    {
        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new AddSupportTicketMessageCommand(Guid.NewGuid(), "Merhaba"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_OnClosedTicket_ReturnsConflict()
    {
        var openerUserId = Guid.NewGuid();
        var assigneeUserId = Guid.NewGuid();
        var ticket = SeedTicket(_supportTicketRepository, openerUserId, assigneeUserId, SupportTicketStatus.Kapandi);

        var result = await CreateHandler(openerUserId).Handle(
            new AddSupportTicketMessageCommand(ticket.Id, "Merhaba"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Empty(_supportTicketMessageRepository.Messages);
    }

    [Fact]
    public async Task Handle_AsAssignee_MarksAnswered_AndNotifiesOpener()
    {
        var openerUserId = Guid.NewGuid();
        var assigneeUserId = Guid.NewGuid();
        var ticket = SeedTicket(_supportTicketRepository, openerUserId, assigneeUserId);
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        _identityService.UserProfilesById[openerUserId] = new IdentityUserProfile(openerUserId, "aday@example.com", "Ahmet", "Yılmaz", null);

        var result = await CreateHandler(assigneeUserId).Handle(
            new AddSupportTicketMessageCommand(ticket.Id, "Yanıt"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SupportTicketStatus.Cevaplandi, ticket.Status);
        var message = Assert.Single(_supportTicketMessageRepository.Messages);
        Assert.Equal(SupportTicketMessageSenderRole.CareerAdvisor, message.SenderRole);
        var notification = Assert.Single(_notificationModuleContract.SentNotifications);
        Assert.Equal(openerUserId, notification.UserId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_AsAssigneeWhoIsNotAnActiveAdvisor_RecordsSenderRoleAsAdmin()
    {
        var openerUserId = Guid.NewGuid();
        var assigneeUserId = Guid.NewGuid();
        var ticket = SeedTicket(_supportTicketRepository, openerUserId, assigneeUserId);
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler(assigneeUserId).Handle(
            new AddSupportTicketMessageCommand(ticket.Id, "Yanıt"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SupportTicketMessageSenderRole.Admin, Assert.Single(_supportTicketMessageRepository.Messages).SenderRole);
    }

    [Fact]
    public async Task Handle_AsOpener_OnAnsweredTicket_Reopens_AndNotifiesAssignee()
    {
        var openerUserId = Guid.NewGuid();
        var assigneeUserId = Guid.NewGuid();
        var ticket = SeedTicket(_supportTicketRepository, openerUserId, assigneeUserId, SupportTicketStatus.Cevaplandi);
        _identityService.UserProfilesById[assigneeUserId] =
            new IdentityUserProfile(assigneeUserId, "danisman@example.com", "Ayşe", "Kaya", null);

        var result = await CreateHandler(openerUserId).Handle(
            new AddSupportTicketMessageCommand(ticket.Id, "Ek bilgi"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SupportTicketStatus.Acik, ticket.Status);
        Assert.Equal(SupportTicketMessageSenderRole.Candidate, Assert.Single(_supportTicketMessageRepository.Messages).SenderRole);
        var notification = Assert.Single(_notificationModuleContract.SentNotifications);
        Assert.Equal(assigneeUserId, notification.UserId);
    }

    [Fact]
    public async Task Handle_AsOpener_OnAcikTicket_DoesNotChangeStatus_ButStillNotifiesAssignee()
    {
        var openerUserId = Guid.NewGuid();
        var assigneeUserId = Guid.NewGuid();
        var ticket = SeedTicket(_supportTicketRepository, openerUserId, assigneeUserId);

        var result = await CreateHandler(openerUserId).Handle(
            new AddSupportTicketMessageCommand(ticket.Id, "Ek bilgi"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SupportTicketStatus.Acik, ticket.Status);
    }

    [Fact]
    public async Task Handle_AsOpener_WithoutAssignee_DoesNotSendNotification()
    {
        var openerUserId = Guid.NewGuid();
        var ticket = SeedTicket(_supportTicketRepository, openerUserId, assignedToUserId: null);

        var result = await CreateHandler(openerUserId).Handle(
            new AddSupportTicketMessageCommand(ticket.Id, "Ek bilgi"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_notificationModuleContract.SentNotifications);
    }
}
