using GenclikMerkezi.Modules.Employment.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employment.Features.GetEmploymentNotes;

public sealed class GetEmploymentNotesQueryHandler(IEmploymentNoteRepository employmentNoteRepository)
    : IRequestHandler<GetEmploymentNotesQuery, Result<GetEmploymentNotesResponse>>
{
    public async Task<Result<GetEmploymentNotesResponse>> Handle(GetEmploymentNotesQuery request, CancellationToken cancellationToken)
    {
        var pagedNotes = await employmentNoteRepository.GetByEmploymentIdAsync(request.EmploymentId, request, cancellationToken);

        var items = pagedNotes.Items
            .Select(n => new EmploymentNoteItemResponse(n.Id, n.CareerAdvisorId, n.Content, n.CreatedAtUtc))
            .ToList();

        return Result.Success(new GetEmploymentNotesResponse(
            items,
            pagedNotes.TotalCount,
            pagedNotes.Page,
            pagedNotes.PageSize,
            pagedNotes.TotalPages,
            pagedNotes.HasNextPage,
            pagedNotes.HasPreviousPage));
    }
}
