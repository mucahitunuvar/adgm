using GenclikMerkezi.Modules.Support.Domain;

namespace GenclikMerkezi.Modules.Support.Features.GetSupportTickets;

// Enum'lar string olarak döner (EmploymentResponse ile aynı gerekçe - System.Text.Json bu projede
// JsonStringEnumConverter yapılandırılmadan sayı döner).
public sealed record SupportTicketSummaryResponse(
    Guid Id,
    string OpenedByRole,
    Guid OpenedByUserId,
    Guid? CandidateCvId,
    Guid? CompanyId,
    string Subject,
    string Priority,
    string Status,
    Guid? AssignedToUserId,
    DateTime OpenedSinceUtc,
    DateTime? ClosedAtUtc,
    Guid? ClosedByUserId,
    string? ClosedReason,
    DateTime CreatedAtUtc)
{
    public static SupportTicketSummaryResponse FromDomain(SupportTicket ticket) => new(
        ticket.Id,
        ticket.OpenedByRole.ToString(),
        ticket.OpenedByUserId,
        ticket.CandidateCvId,
        ticket.CompanyId,
        ticket.Subject,
        ticket.Priority.ToString(),
        ticket.Status.ToString(),
        ticket.AssignedToUserId,
        ticket.OpenedSinceUtc,
        ticket.ClosedAtUtc,
        ticket.ClosedByUserId,
        ticket.ClosedReason,
        ticket.CreatedAtUtc);
}
