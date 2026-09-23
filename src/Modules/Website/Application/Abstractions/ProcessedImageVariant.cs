namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public sealed record ProcessedImageVariant(string VariantName, byte[] Bytes, int Width, int Height);
