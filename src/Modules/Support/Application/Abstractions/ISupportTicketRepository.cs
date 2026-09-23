using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Support.Application.Abstractions;

public interface ISupportTicketRepository
{
    Task<SupportTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // openedByUserId: null ise çağıran CareerAdvisor/Admin'dir ve tüm talepler döner ("herkes görsün"
    // kararı); Candidate/Employer için kendi UserId'si geçilir ve DB seviyesinde filtrelenir (AGENTS.md
    // §40 - tüm talepleri çekip bellekte filtrelemek yerine). Bu, task'ın "geniş filtre gerekmiyor"
    // notunun kastettiği çok alanlı bir filtre nesnesi değil, tek bir opsiyonel sahiplik id'si.
    Task<PagedResult<SupportTicket>> GetAllAsync(
        Guid? openedByUserId, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // CloseOverdueSupportTicketsJob (Hangfire) tarafından kullanılır - Acik durumda ve
    // OpenedSinceUtc <= threshold olan talepler.
    Task<IReadOnlyList<SupportTicket>> GetOverdueOpenTicketsAsync(DateTime threshold, CancellationToken cancellationToken = default);

    void Add(SupportTicket supportTicket);
}
