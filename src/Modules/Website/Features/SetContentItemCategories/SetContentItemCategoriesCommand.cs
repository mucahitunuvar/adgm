using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemCategories;

public sealed record SetContentItemCategoriesCommand(Guid Id, byte[] RowVersion, IReadOnlyList<Guid> CategoryIds) : IRequest<Result>;
