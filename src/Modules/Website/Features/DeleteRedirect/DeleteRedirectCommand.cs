using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteRedirect;

public sealed record DeleteRedirectCommand(Guid Id) : IRequest<Result>;
