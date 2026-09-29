using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentCategory;

public sealed record UpdateContentCategoryCommand(Guid TypeId, Guid Id, byte[] RowVersion, int SortOrder) : IRequest<Result>;
