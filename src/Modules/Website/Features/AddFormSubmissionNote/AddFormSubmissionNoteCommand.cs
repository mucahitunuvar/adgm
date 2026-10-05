using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.AddFormSubmissionNote;

public sealed record AddFormSubmissionNoteCommand(Guid Id, byte[] RowVersion, string Text) : IRequest<Result>;
