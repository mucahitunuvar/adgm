using GenclikMerkezi.Modules.Support.Domain;

namespace GenclikMerkezi.Modules.Support.Features.GetSupportTicketMessages;

public sealed record SupportTicketMessageItemResponse(Guid Id, Guid SenderUserId, string SenderRole, string Content, DateTime CreatedAtUtc)
{
    public static SupportTicketMessageItemResponse FromDomain(SupportTicketMessage message) => new(
        message.Id, message.SenderUserId, message.SenderRole.ToString(), message.Content, message.CreatedAtUtc);
}
