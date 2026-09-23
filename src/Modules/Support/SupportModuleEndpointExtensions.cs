using GenclikMerkezi.Modules.Support.Features.AddSupportTicketMessage;
using GenclikMerkezi.Modules.Support.Features.ChangeSupportTicketPriority;
using GenclikMerkezi.Modules.Support.Features.CloseSupportTicket;
using GenclikMerkezi.Modules.Support.Features.CreateSupportTicket;
using GenclikMerkezi.Modules.Support.Features.GetSupportTicketMessages;
using GenclikMerkezi.Modules.Support.Features.GetSupportTickets;
using GenclikMerkezi.Modules.Support.Features.TransferSupportTicket;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Support;

public static class SupportModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapSupportModuleEndpoints(this IEndpointRouteBuilder app)
    {
        CreateSupportTicketEndpoint.Map(app);
        AddSupportTicketMessageEndpoint.Map(app);
        TransferSupportTicketEndpoint.Map(app);
        ChangeSupportTicketPriorityEndpoint.Map(app);
        CloseSupportTicketEndpoint.Map(app);
        GetSupportTicketsEndpoint.Map(app);
        GetSupportTicketMessagesEndpoint.Map(app);

        return app;
    }
}
