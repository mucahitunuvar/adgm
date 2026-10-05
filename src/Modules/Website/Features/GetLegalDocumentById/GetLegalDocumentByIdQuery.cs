using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;

public sealed record GetLegalDocumentByIdQuery(Guid Id) : IRequest<Result<LegalDocumentDetailResponse>>;
