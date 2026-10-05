using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemForm;

public sealed record SetContentItemFormCommand(Guid Id, byte[] RowVersion, Guid? FormDefinitionId) : IRequest<Result>;
