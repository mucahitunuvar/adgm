using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissions;

public sealed class GetFormSubmissionsQueryHandler(IFormSubmissionRepository formSubmissionRepository)
    : IRequestHandler<GetFormSubmissionsQuery, Result<PagedResult<FormSubmissionSummaryResponse>>>
{
    public async Task<Result<PagedResult<FormSubmissionSummaryResponse>>> Handle(
        GetFormSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var filter = new FormSubmissionSearchFilter(
            request.FormKey, request.Status, request.Archived, request.AssignedToUserId, request.From, request.To,
            request.ReferenceNumber);

        var paged = await formSubmissionRepository.SearchAsync(filter, request, cancellationToken);

        var items = paged.Items
            .Select(i => new FormSubmissionSummaryResponse(
                i.Id, i.ReferenceNumber, i.FormKey.Value, i.Status, i.SubmittedAtUtc, i.AssignedToUserId, i.AttachmentCount, i.IsArchived,
                i.RowVersion))
            .ToList();

        return Result.Success(new PagedResult<FormSubmissionSummaryResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
