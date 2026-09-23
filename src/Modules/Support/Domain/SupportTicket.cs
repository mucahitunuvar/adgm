using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Support.Domain;

// Aggregate root (AGENTS.md §10). CandidateCvId/CompanyId, Candidate/Employer modüllerine
// cross-module referans - Employment.cs'teki desenle aynı, FK yok, doğrulanmaz. AssignedToUserId bir
// CareerAdvisorId DEĞİL, doğrudan Identity.User.Id'dir (danışman ya da admin olabilir) - bildirim
// göndermek için IIdentityService.GetUserProfileAsync ile doğrudan çözülür, ayrı bir "hangi rol"
// çözümlemesine gerek kalmaz (CandidateNote/Employment'ın CareerAdvisorId'sinden bilinçli sapma).
public sealed class SupportTicket : AggregateRoot
{
    public SupportTicketOpenerRole OpenedByRole { get; private set; }

    public Guid OpenedByUserId { get; private set; }

    public Guid? CandidateCvId { get; private set; }

    public Guid? CompanyId { get; private set; }

    public string Subject { get; private set; }

    public SupportTicketPriority Priority { get; private set; }

    public SupportTicketStatus Status { get; private set; }

    public Guid? AssignedToUserId { get; private set; }

    // SLA hesaplaması bunun üzerinden yapılır (CloseOverdueSupportTicketsJob) - oluşturulunca ve
    // Cevaplandi'dan Acik'e her ReopenByFollowUp'ta sıfırlanır.
    public DateTime OpenedSinceUtc { get; private set; }

    public DateTime? ClosedAtUtc { get; private set; }

    // null = sistem (SLA job'ı) otomatik kapattı, dolu = bir CareerAdvisor/Admin manuel kapattı.
    public Guid? ClosedByUserId { get; private set; }

    public string? ClosedReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private SupportTicket(
        Guid id,
        SupportTicketOpenerRole openedByRole,
        Guid openedByUserId,
        Guid? candidateCvId,
        Guid? companyId,
        string subject,
        SupportTicketPriority priority,
        Guid? assignedToUserId,
        DateTime createdAtUtc)
        : base(id)
    {
        OpenedByRole = openedByRole;
        OpenedByUserId = openedByUserId;
        CandidateCvId = candidateCvId;
        CompanyId = companyId;
        Subject = subject;
        Priority = priority;
        Status = SupportTicketStatus.Acik;
        AssignedToUserId = assignedToUserId;
        OpenedSinceUtc = createdAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public static SupportTicket Create(
        SupportTicketOpenerRole openedByRole,
        Guid openedByUserId,
        Guid? candidateCvId,
        Guid? companyId,
        string subject,
        SupportTicketPriority priority,
        Guid? assignedToUserId,
        DateTime createdAtUtc) =>
        new(Guid.NewGuid(), openedByRole, openedByUserId, candidateCvId, companyId, subject, priority, assignedToUserId, createdAtUtc);

    public Result MarkAnswered(DateTime respondedAtUtc)
    {
        if (Status != SupportTicketStatus.Acik)
        {
            return Result.Failure(Error.Conflict(
                "SupportTicket.InvalidTransition", $"Cannot mark a ticket as answered while status is {Status}."));
        }

        Status = SupportTicketStatus.Cevaplandi;

        return Result.Success();
    }

    public Result ReopenByFollowUp(DateTime reopenedAtUtc)
    {
        if (Status != SupportTicketStatus.Cevaplandi)
        {
            return Result.Failure(Error.Conflict(
                "SupportTicket.InvalidTransition", $"Cannot reopen a ticket while status is {Status}."));
        }

        Status = SupportTicketStatus.Acik;
        OpenedSinceUtc = reopenedAtUtc;

        return Result.Success();
    }

    public Result Transfer(Guid newAssigneeUserId)
    {
        if (Status == SupportTicketStatus.Kapandi)
        {
            return Result.Failure(Error.Conflict("SupportTicket.AlreadyClosed", "Cannot transfer a closed ticket."));
        }

        AssignedToUserId = newAssigneeUserId;

        return Result.Success();
    }

    public Result ChangePriority(SupportTicketPriority newPriority)
    {
        if (Status == SupportTicketStatus.Kapandi)
        {
            return Result.Failure(
                Error.Conflict("SupportTicket.AlreadyClosed", "Cannot change the priority of a closed ticket."));
        }

        Priority = newPriority;

        return Result.Success();
    }

    public Result Close(Guid? closedByUserId, string? reason, DateTime closedAtUtc)
    {
        if (Status == SupportTicketStatus.Kapandi)
        {
            return Result.Failure(Error.Conflict("SupportTicket.AlreadyClosed", "This ticket is already closed."));
        }

        Status = SupportTicketStatus.Kapandi;
        ClosedByUserId = closedByUserId;
        ClosedReason = reason;
        ClosedAtUtc = closedAtUtc;

        return Result.Success();
    }
}
