using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §6: one resized rendition of an image MediaAsset (small/medium/large - see
// MediaAssetVariantName). Documents have no variants.
public sealed class MediaAssetVariant : ValueObject
{
    public string VariantName { get; } = string.Empty;

    public FileAttachment File { get; } = null!;

    public int Width { get; }

    public int Height { get; }

    private MediaAssetVariant(string variantName, FileAttachment file, int width, int height)
    {
        VariantName = variantName;
        File = file;
        Width = width;
        Height = height;
    }

    // EF Core cannot constructor-bind a reference to another owned type (File) - only scalar
    // constructor parameters - so this parameterless constructor lets EF materialize the instance
    // via backing-field access instead, then fix up File as a nested owned navigation.
    private MediaAssetVariant()
    {
    }

    public static MediaAssetVariant Create(string variantName, FileAttachment file, int width, int height) =>
        new(variantName, file, width, height);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return VariantName;
        yield return File;
        yield return Width;
        yield return Height;
    }
}
