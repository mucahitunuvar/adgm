using GenclikMerkezi.Modules.Support.Domain;

namespace GenclikMerkezi.UnitTests.Support.Domain;

public class SupportTicketMessageTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var supportTicketId = Guid.NewGuid();
        var senderUserId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var message = SupportTicketMessage.Create(
            supportTicketId, senderUserId, SupportTicketMessageSenderRole.CareerAdvisor, "Merhaba", createdAtUtc);

        Assert.Equal(supportTicketId, message.SupportTicketId);
        Assert.Equal(senderUserId, message.SenderUserId);
        Assert.Equal(SupportTicketMessageSenderRole.CareerAdvisor, message.SenderRole);
        Assert.Equal("Merhaba", message.Content);
        Assert.Equal(createdAtUtc, message.CreatedAtUtc);
    }
}
