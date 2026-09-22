using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employment.Features.GetEmploymentNotes;

public sealed record GetEmploymentNotesQuery(Guid EmploymentId) : PagedRequest, IRequest<Result<GetEmploymentNotesResponse>>;
