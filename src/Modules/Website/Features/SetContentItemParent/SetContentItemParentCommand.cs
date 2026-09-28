using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemParent;

public sealed record SetContentItemParentCommand(Guid Id, byte[] RowVersion, Guid? ParentId) : IRequest<Result>;
