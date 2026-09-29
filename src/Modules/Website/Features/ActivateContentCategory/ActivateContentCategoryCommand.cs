using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ActivateContentCategory;

public sealed record ActivateContentCategoryCommand(Guid TypeId, Guid Id, byte[] RowVersion) : IRequest<Result>;
