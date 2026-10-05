using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateFormDefinition;

public sealed record DeactivateFormDefinitionCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
