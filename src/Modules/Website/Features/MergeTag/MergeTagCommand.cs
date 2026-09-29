using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.MergeTag;

public sealed record MergeTagCommand(Guid Id, Guid TargetId) : IRequest<Result>;
