using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicLegalDocumentVersion;

public sealed record GetPublicLegalDocumentVersionQuery(
    string Key, int VersionNumber, string? Lang) : IRequest<Result<PublicLegalDocumentVersionResponse>>;
