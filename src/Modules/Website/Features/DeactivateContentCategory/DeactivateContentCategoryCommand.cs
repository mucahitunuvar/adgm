using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateContentCategory;

public sealed record DeactivateContentCategoryCommand(Guid TypeId, Guid Id, byte[] RowVersion) : IRequest<Result>;
