namespace GenclikMerkezi.Modules.Website.Features.ReplaceSlides;

public sealed record ReplaceSlidesRequest(byte[] RowVersion, IReadOnlyList<SlideInput> Slides);
