using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetLegalDocuments;

public sealed record GetLegalDocumentsQuery : IRequest<Result<IReadOnlyList<LegalDocumentSummaryResponse>>>;
