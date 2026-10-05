using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissions;

// ADR-024 §12.2 (Faz 3 Görev 5): archived defaults to false at the endpoint (query-string binding), not
// here, matching PagedRequest's own "defaults live at the edge" shape.
public sealed record GetFormSubmissionsQuery(
    string? FormKey,
    string? Status,
    bool Archived,
    Guid? AssignedToUserId,
    DateTime? From,
    DateTime? To,
    string? ReferenceNumber) : PagedRequest, IRequest<Result<PagedResult<FormSubmissionSummaryResponse>>>;
