using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetFormDefinitions;

public sealed record GetFormDefinitionsQuery : IRequest<Result<IReadOnlyList<FormDefinitionSummaryResponse>>>;
