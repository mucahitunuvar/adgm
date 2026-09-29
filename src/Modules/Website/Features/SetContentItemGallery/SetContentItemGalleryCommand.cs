using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemGallery;

public sealed record SetContentItemGalleryCommand(Guid Id, byte[] RowVersion, IReadOnlyList<GalleryItemInput> Items) : IRequest<Result>;
