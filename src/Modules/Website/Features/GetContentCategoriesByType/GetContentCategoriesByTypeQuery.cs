using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentCategoriesByType;

public sealed record GetContentCategoriesByTypeQuery(Guid TypeId) : IRequest<Result<IReadOnlyList<ContentCategoryTreeItemResponse>>>;
