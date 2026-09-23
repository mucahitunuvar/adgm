using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Support.Features.GetSupportTicketMessages;

// GetEmploymentNotesQueryHandler'daki desenle aynı: talebin var olup olmadığı ayrıca kontrol edilmez,
// repository geçersiz bir id için boş bir sayfa döner. Mesaj içeriğini görüntülemek için kısıtlama
// yok (task tanımı: "herkes görebilir - mesaj içeriği görmek yanıtlamaktan farklı").
public sealed class GetSupportTicketMessagesQueryHandler(ISupportTicketMessageRepository supportTicketMessageRepository)
    : IRequestHandler<GetSupportTicketMessagesQuery, Result<GetSupportTicketMessagesResponse>>
{
    public async Task<Result<GetSupportTicketMessagesResponse>> Handle(
        GetSupportTicketMessagesQuery request, CancellationToken cancellationToken)
    {
        var pagedMessages = await supportTicketMessageRepository.GetByTicketIdAsync(request.SupportTicketId, request, cancellationToken);

        var items = pagedMessages.Items.Select(SupportTicketMessageItemResponse.FromDomain).ToList();

        return Result.Success(new GetSupportTicketMessagesResponse(
            items,
            pagedMessages.TotalCount,
            pagedMessages.Page,
            pagedMessages.PageSize,
            pagedMessages.TotalPages,
            pagedMessages.HasNextPage,
            pagedMessages.HasPreviousPage));
    }
}
