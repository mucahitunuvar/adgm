using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeImageProcessor : IImageProcessor
{
    public Result<ProcessedImage> ResultToReturn { get; set; } = Result.Success(new ProcessedImage(
        [1, 2, 3], 800, 600, [new ProcessedImageVariant("small", [4, 5, 6], 400, 300)]));

    public byte[]? LastBytesReceived { get; private set; }

    public Result<ProcessedImage> Process(byte[] originalBytes)
    {
        LastBytesReceived = originalBytes;
        return ResultToReturn;
    }
}
