using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentTypeById;

public sealed record GetContentTypeByIdQuery(Guid Id) : IRequest<Result<ContentTypeDetailResponse>>;
