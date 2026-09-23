using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §6 / AGENTS.md §7: Application must not depend on a concrete Infrastructure
// implementation, so the SkiaSharp-based processing (SkiaSharpImageProcessor) sits behind this port.
// A Result.Failure here doubles as the "is this actually a valid image" check (ADR-024 Faz 0 Görev
// 5): content that merely has an image extension/content-type but fails to decode is rejected here,
// not upstream.
public interface IImageProcessor
{
    Result<ProcessedImage> Process(byte[] originalBytes);
}
