using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentCategory;

public sealed record DeleteContentCategoryCommand(Guid TypeId, Guid Id) : IRequest<Result>;
