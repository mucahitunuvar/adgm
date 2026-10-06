using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetThirdPartyScriptById;

public sealed record GetThirdPartyScriptByIdQuery(Guid Id) : IRequest<Result<ThirdPartyScriptDetailResponse>>;
