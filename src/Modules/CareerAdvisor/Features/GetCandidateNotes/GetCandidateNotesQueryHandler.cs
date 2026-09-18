using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.GetCandidateNotes;

public sealed class GetCandidateNotesQueryHandler(ICandidateNoteRepository candidateNoteRepository)
    : IRequestHandler<GetCandidateNotesQuery, Result<GetCandidateNotesResponse>>
{
    public async Task<Result<GetCandidateNotesResponse>> Handle(GetCandidateNotesQuery request, CancellationToken cancellationToken)
    {
        var pagedNotes = await candidateNoteRepository.GetByCandidateCvIdAsync(request.CandidateCvId, request, cancellationToken);

        var items = pagedNotes.Items
            .Select(n => new CandidateNoteItemResponse(n.Id, n.CareerAdvisorId, n.NoteType.ToString(), n.Content, n.CreatedAtUtc))
            .ToList();

        return Result.Success(new GetCandidateNotesResponse(
            items,
            pagedNotes.TotalCount,
            pagedNotes.Page,
            pagedNotes.PageSize,
            pagedNotes.TotalPages,
            pagedNotes.HasNextPage,
            pagedNotes.HasPreviousPage));
    }
}
