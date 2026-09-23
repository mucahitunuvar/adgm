using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.Modules.Support.Features.TransferSupportTicket;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Support.TestDoubles;

namespace GenclikMerkezi.UnitTests.Support.Features.TransferSupportTicket;

public class TransferSupportTicketCommandHandlerTests
{
    private readonly FakeSupportTicketRepository _supportTicketRepository = new();
    private readonly FakeIdentityService _identityService = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private TransferSupportTicketCommandHandler CreateHandler(Guid callerUserId) =>
        new(_supportTicketRepository, _identityService, _notificationModuleContract, new FakeCurrentUserContext(callerUserId), _unitOfWork);

    private SupportTicket SeedTicket(Guid assignedToUserId)
    {
        var ticket = SupportTicket.Create(
            SupportTicketOpenerRole.Candidate, Guid.NewGuid(), Guid.NewGuid(), null,
            "Konu", SupportTicketPriority.Orta, assignedToUserId, DateTime.UtcNow);
        _supportTicketRepository.Add(ticket);
        return ticket;
    }

    [Fact]
    public async Task Handle_AsCurrentAssignee_Transfers_AndNotifiesNewAssignee()
    {
        var currentAssigneeUserId = Guid.NewGuid();
        var newAssigneeUserId = Guid.NewGuid();
        var ticket = SeedTicket(currentAssigneeUserId);
        _identityService.UserProfilesById[newAssigneeUserId] =
            new IdentityUserProfile(newAssigneeUserId, "yeni-danisman@example.com", "Mehmet", "Demir", null);

        var result = await CreateHandler(currentAssigneeUserId).Handle(
            new TransferSupportTicketCommand(ticket.Id, newAssigneeUserId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(newAssigneeUserId, ticket.AssignedToUserId);
        var notification = Assert.Single(_notificationModuleContract.SentNotifications);
        Assert.Equal(newAssigneeUserId, notification.UserId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotCurrentAssignee_ReturnsForbidden()
    {
        var currentAssigneeUserId = Guid.NewGuid();
        var ticket = SeedTicket(currentAssigneeUserId);

        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new TransferSupportTicketCommand(ticket.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(currentAssigneeUserId, ticket.AssignedToUserId);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownTicketId_ReturnsNotFound()
    {
        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new TransferSupportTicketCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
