namespace GenclikMerkezi.Modules.Support.Features.GetSupportTicketMessages;

public sealed record GetSupportTicketMessagesResponse(
    IReadOnlyList<SupportTicketMessageItemResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);
