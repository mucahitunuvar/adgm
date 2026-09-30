using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicContentById;

public sealed record GetPublicContentByIdQuery(Guid Id, string? Lang) : IRequest<Result<PublicContentDetailResponse>>;
