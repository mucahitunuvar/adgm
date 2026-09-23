using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Support.Features.GetSupportTickets;

// Rol, bir Identity claim'i olarak değil (CreateSupportTicketCommandHandler'daki aynı desen) hangi
// modülün profil kaydına sahip olduğuna göre çözülür: Candidate/Employer kendi profiline sahipse
// yalnızca kendi açtığı talepler (DB seviyesinde filtrelenir), CareerAdvisor/Admin'in (hiçbirinin
// profili yok) tümü görür - task tanımındaki "herkes görsün" kararı.
public sealed class GetSupportTicketsQueryHandler(
    ISupportTicketRepository supportTicketRepository,
    ICandidateModuleContract candidateModuleContract,
    ICompanyModuleContract companyModuleContract,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<GetSupportTicketsQuery, Result<GetSupportTicketsResponse>>
{
    public async Task<Result<GetSupportTicketsResponse>> Handle(GetSupportTicketsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.UserId!.Value;

        var hasOwnProfile = await candidateModuleContract.GetCandidateCvByUserIdAsync(userId, cancellationToken) is not null
            || await companyModuleContract.GetCompanyByUserIdAsync(userId, cancellationToken) is not null;

        var openedByUserIdFilter = hasOwnProfile ? userId : (Guid?)null;

        var pagedTickets = await supportTicketRepository.GetAllAsync(openedByUserIdFilter, request, cancellationToken);

        var items = pagedTickets.Items.Select(SupportTicketSummaryResponse.FromDomain).ToList();

        return Result.Success(new GetSupportTicketsResponse(
            items,
            pagedTickets.TotalCount,
            pagedTickets.Page,
            pagedTickets.PageSize,
            pagedTickets.TotalPages,
            pagedTickets.HasNextPage,
            pagedTickets.HasPreviousPage));
    }
}
