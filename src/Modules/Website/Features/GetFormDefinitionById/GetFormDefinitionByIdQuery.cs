using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetFormDefinitionById;

public sealed record GetFormDefinitionByIdQuery(Guid Id) : IRequest<Result<FormDefinitionDetailResponse>>;
