using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.GetHomeLayout;

public sealed record GetHomeLayoutResponse(
    IReadOnlyList<LayoutBlockResponse> DraftBlocks,
    IReadOnlyList<LayoutBlockResponse> PublishedBlocks,
    bool HasUnpublishedChanges,
    DateTime? PublishedAtUtc,
    byte[] RowVersion)
{
    public static GetHomeLayoutResponse FromDomain(PageLayout layout) =>
        new(
            layout.DraftBlocks.Select(LayoutBlockResponse.FromDomain).ToList(),
            layout.PublishedBlocks.Select(LayoutBlockResponse.FromDomain).ToList(),
            layout.HasUnpublishedChanges,
            layout.PublishedAtUtc,
            layout.RowVersion);
}
