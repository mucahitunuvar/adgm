using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Support.Domain;

public class SupportTicketTests
{
    private static SupportTicket CreateTicket(Guid? assignedToUserId = null) =>
        SupportTicket.Create(
            SupportTicketOpenerRole.Candidate, Guid.NewGuid(), Guid.NewGuid(), null,
            "Bir sorun yaşıyorum", SupportTicketPriority.Orta, assignedToUserId, DateTime.UtcNow);

    [Fact]
    public void Create_DefaultsToAcik_AndSetsOpenedSinceUtcToCreatedAtUtc()
    {
        var createdAtUtc = DateTime.UtcNow;
        var ticket = SupportTicket.Create(
            SupportTicketOpenerRole.Employer, Guid.NewGuid(), null, Guid.NewGuid(),
            "Konu", SupportTicketPriority.Acil, null, createdAtUtc);

        Assert.Equal(SupportTicketStatus.Acik, ticket.Status);
        Assert.Equal(createdAtUtc, ticket.OpenedSinceUtc);
        Assert.Equal(createdAtUtc, ticket.CreatedAtUtc);
        Assert.Null(ticket.ClosedAtUtc);
        Assert.Null(ticket.ClosedByUserId);
        Assert.Null(ticket.ClosedReason);
    }

    [Fact]
    public void MarkAnswered_FromAcik_Succeeds()
    {
        var ticket = CreateTicket();

        var result = ticket.MarkAnswered(DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(SupportTicketStatus.Cevaplandi, ticket.Status);
    }

    [Fact]
    public void MarkAnswered_FromCevaplandi_Fails()
    {
        var ticket = CreateTicket();
        ticket.MarkAnswered(DateTime.UtcNow);

        var result = ticket.MarkAnswered(DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void MarkAnswered_FromKapandi_Fails()
    {
        var ticket = CreateTicket();
        ticket.Close(Guid.NewGuid(), "Kapatıldı", DateTime.UtcNow);

        var result = ticket.MarkAnswered(DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void ReopenByFollowUp_FromCevaplandi_Succeeds_AndResetsOpenedSinceUtc()
    {
        var ticket = CreateTicket();
        ticket.MarkAnswered(DateTime.UtcNow);
        var reopenedAtUtc = DateTime.UtcNow.AddHours(1);

        var result = ticket.ReopenByFollowUp(reopenedAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(SupportTicketStatus.Acik, ticket.Status);
        Assert.Equal(reopenedAtUtc, ticket.OpenedSinceUtc);
    }

    [Fact]
    public void ReopenByFollowUp_FromAcik_Fails()
    {
        var ticket = CreateTicket();

        var result = ticket.ReopenByFollowUp(DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void ReopenByFollowUp_FromKapandi_Fails()
    {
        var ticket = CreateTicket();
        ticket.Close(Guid.NewGuid(), null, DateTime.UtcNow);

        var result = ticket.ReopenByFollowUp(DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Transfer_WhileNotClosed_Succeeds(bool answeredFirst)
    {
        var ticket = CreateTicket();

        if (answeredFirst)
        {
            ticket.MarkAnswered(DateTime.UtcNow);
        }

        var newAssigneeUserId = Guid.NewGuid();

        var result = ticket.Transfer(newAssigneeUserId);

        Assert.True(result.IsSuccess);
        Assert.Equal(newAssigneeUserId, ticket.AssignedToUserId);
    }

    [Fact]
    public void Transfer_WhenClosed_Fails()
    {
        var ticket = CreateTicket();
        ticket.Close(Guid.NewGuid(), null, DateTime.UtcNow);
        var originalAssignee = ticket.AssignedToUserId;

        var result = ticket.Transfer(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(originalAssignee, ticket.AssignedToUserId);
    }

    [Fact]
    public void ChangePriority_WhileNotClosed_Succeeds()
    {
        var ticket = CreateTicket();

        var result = ticket.ChangePriority(SupportTicketPriority.Acil);

        Assert.True(result.IsSuccess);
        Assert.Equal(SupportTicketPriority.Acil, ticket.Priority);
    }

    [Fact]
    public void ChangePriority_WhenClosed_Fails()
    {
        var ticket = CreateTicket();
        ticket.Close(Guid.NewGuid(), null, DateTime.UtcNow);

        var result = ticket.ChangePriority(SupportTicketPriority.Acil);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(SupportTicketPriority.Orta, ticket.Priority);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Close_FromAnyNonClosedStatus_Succeeds(bool answeredFirst)
    {
        var ticket = CreateTicket();

        if (answeredFirst)
        {
            ticket.MarkAnswered(DateTime.UtcNow);
        }

        var closedByUserId = Guid.NewGuid();
        var closedAtUtc = DateTime.UtcNow;

        var result = ticket.Close(closedByUserId, "Çözüldü", closedAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(SupportTicketStatus.Kapandi, ticket.Status);
        Assert.Equal(closedByUserId, ticket.ClosedByUserId);
        Assert.Equal("Çözüldü", ticket.ClosedReason);
        Assert.Equal(closedAtUtc, ticket.ClosedAtUtc);
    }

    [Fact]
    public void Close_WithNullClosedByUserId_RecordsSystemClosure()
    {
        var ticket = CreateTicket();

        var result = ticket.Close(null, "SLA süresi doldu, otomatik kapatıldı.", DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Null(ticket.ClosedByUserId);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_Fails()
    {
        var ticket = CreateTicket();
        ticket.Close(Guid.NewGuid(), "İlk neden", DateTime.UtcNow);

        var result = ticket.Close(Guid.NewGuid(), "İkinci neden", DateTime.UtcNow.AddHours(1));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("İlk neden", ticket.ClosedReason);
    }
}
