using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteFormDefinition;

public sealed record DeleteFormDefinitionCommand(Guid Id) : IRequest<Result>;
