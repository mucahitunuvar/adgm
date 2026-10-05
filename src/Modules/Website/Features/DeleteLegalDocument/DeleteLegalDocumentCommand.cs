using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteLegalDocument;

public sealed record DeleteLegalDocumentCommand(Guid Id) : IRequest<Result>;
