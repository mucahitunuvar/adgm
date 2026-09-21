using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.GetGeneralPool;

public sealed record GetGeneralPoolQuery : PagedRequest, IRequest<Result<GetGeneralPoolResponse>>;
