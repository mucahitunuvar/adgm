using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ActivateThirdPartyScript;

public sealed record ActivateThirdPartyScriptCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
