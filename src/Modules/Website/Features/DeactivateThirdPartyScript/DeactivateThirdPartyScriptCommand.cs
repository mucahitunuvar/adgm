using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateThirdPartyScript;

public sealed record DeactivateThirdPartyScriptCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
