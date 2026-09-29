using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemRelatedContent;

public sealed record SetContentItemRelatedContentCommand(Guid Id, byte[] RowVersion, IReadOnlyList<Guid> RelatedContentItemIds) : IRequest<Result>;
