using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentItem;

public sealed record UpdateContentItemCommand(
    Guid Id, byte[] RowVersion, int SortOrder, bool IsFeatured, Guid? CoverImageMediaId, Guid? DetailImageMediaId) : IRequest<Result>;
