using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteThirdPartyScript;

public sealed record DeleteThirdPartyScriptCommand(Guid Id) : IRequest<Result>;
