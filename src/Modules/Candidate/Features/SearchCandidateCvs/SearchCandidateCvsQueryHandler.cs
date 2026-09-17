using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.SearchCandidateCvs;

public sealed class SearchCandidateCvsQueryHandler(ICandidateCvRepository candidateCvRepository)
    : IRequestHandler<SearchCandidateCvsQuery, Result<SearchCandidateCvsResponse>>
{
    public async Task<Result<SearchCandidateCvsResponse>> Handle(SearchCandidateCvsQuery request, CancellationToken cancellationToken)
    {
        var filter = new CandidateCvSearchFilter(request.SearchText, request.Page, request.PageSize);
        var pagedCandidates = await candidateCvRepository.SearchAsync(filter, cancellationToken);

        var items = pagedCandidates.Items
            .Select(c => new CandidateCvListItemResponse(c.Id, c.UserId, c.FirstName, c.LastName, c.Email, c.CompletionPercentage))
            .ToList();

        return Result.Success(new SearchCandidateCvsResponse(
            items,
            pagedCandidates.TotalCount,
            pagedCandidates.Page,
            pagedCandidates.PageSize,
            pagedCandidates.TotalPages,
            pagedCandidates.HasNextPage,
            pagedCandidates.HasPreviousPage));
    }
}
