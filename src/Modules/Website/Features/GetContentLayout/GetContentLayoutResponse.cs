using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.GetContentLayout;

public sealed record GetContentLayoutResponse(
    IReadOnlyList<LayoutBlockResponse> DraftBlocks,
    IReadOnlyList<LayoutBlockResponse> PublishedBlocks,
    bool HasUnpublishedChanges,
    DateTime? PublishedAtUtc,
    byte[] RowVersion)
{
    // §4.3 "İçerik için düzen yoksa boş taslak döner (oluşturulmamış sayılır)" - an empty RowVersion
    // is the sentinel ReplaceContentDraftBlocksCommandHandler recognizes as "nothing to conflict
    // against yet", mirroring back whatever the client just received here.
    public static GetContentLayoutResponse Empty { get; } = new([], [], false, null, []);

    public static GetContentLayoutResponse FromDomain(PageLayout layout) =>
        new(
            layout.DraftBlocks.Select(LayoutBlockResponse.FromDomain).ToList(),
            layout.PublishedBlocks.Select(LayoutBlockResponse.FromDomain).ToList(),
            layout.HasUnpublishedChanges,
            layout.PublishedAtUtc,
            layout.RowVersion);
}
