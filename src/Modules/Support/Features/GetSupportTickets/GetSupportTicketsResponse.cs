namespace GenclikMerkezi.Modules.Support.Features.GetSupportTickets;

public sealed record GetSupportTicketsResponse(
    IReadOnlyList<SupportTicketSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);
