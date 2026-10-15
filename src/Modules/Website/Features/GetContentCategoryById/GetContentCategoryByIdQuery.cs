using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentCategoryById;

public sealed record GetContentCategoryByIdQuery(Guid TypeId, Guid Id) : IRequest<Result<ContentCategoryDetailResponse>>;
