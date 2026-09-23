using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Support.Domain;

// CandidateNote.cs deseninin kopyası (bağımsız, append-only mesaj akışı - ayrı bir aggregate root,
// SupportTicket'ın alt koleksiyonu değil). SupportTicketId, aynı modülün kendi aggregate'ine
// referans - cross-module bir id değil.
public sealed class SupportTicketMessage : AggregateRoot
{
    public Guid SupportTicketId { get; private set; }

    public Guid SenderUserId { get; private set; }

    public SupportTicketMessageSenderRole SenderRole { get; private set; }

    public string Content { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private SupportTicketMessage(
        Guid id,
        Guid supportTicketId,
        Guid senderUserId,
        SupportTicketMessageSenderRole senderRole,
        string content,
        DateTime createdAtUtc)
        : base(id)
    {
        SupportTicketId = supportTicketId;
        SenderUserId = senderUserId;
        SenderRole = senderRole;
        Content = content;
        CreatedAtUtc = createdAtUtc;
    }

    public static SupportTicketMessage Create(
        Guid supportTicketId, Guid senderUserId, SupportTicketMessageSenderRole senderRole, string content, DateTime createdAtUtc) =>
        new(Guid.NewGuid(), supportTicketId, senderUserId, senderRole, content, createdAtUtc);
}
