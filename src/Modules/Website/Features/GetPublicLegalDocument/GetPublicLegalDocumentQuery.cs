using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicLegalDocument;

public sealed record GetPublicLegalDocumentQuery(string Key, string? Lang) : IRequest<Result<PublicLegalDocumentResponse>>;
