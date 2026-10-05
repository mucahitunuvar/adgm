using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ActivateFormDefinition;

public sealed record ActivateFormDefinitionCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
