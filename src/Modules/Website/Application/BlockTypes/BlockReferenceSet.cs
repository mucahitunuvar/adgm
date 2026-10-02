namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// Every entity a single block's Settings/Texts can reference, bundled as one return value from
// IBlockTypeDefinition.GetReferences (§4.1 "referans verdiği varlıkların listesini çıkaran bir
// metot") - PageLayoutReferenceValidator merges every block's set and checks existence in bulk, one
// query per kind across the whole layout, not per block.
public sealed record BlockReferenceSet(
    IReadOnlyList<Guid> ImageMediaIds,
    IReadOnlyList<Guid> VideoIds,
    IReadOnlyList<Guid> SliderIds,
    IReadOnlyList<Guid> ImpactMetricIds,
    IReadOnlyList<Guid> ContentItemIds,
    IReadOnlyList<Guid> ContentTypeListingIds,
    IReadOnlyList<ContentTypeCategoryReference> ContentTypeReferences,
    IReadOnlyList<string> EventContentTypeKeys)
{
    public static BlockReferenceSet Empty { get; } = new([], [], [], [], [], [], [], []);
}
