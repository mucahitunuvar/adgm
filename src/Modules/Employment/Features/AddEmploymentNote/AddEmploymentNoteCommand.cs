using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employment.Features.AddEmploymentNote;

public sealed record AddEmploymentNoteCommand(Guid EmploymentId, string Content) : IRequest<Result<AddEmploymentNoteResponse>>;
