using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteTag;

public sealed record DeleteTagCommand(Guid Id) : IRequest<Result>;
