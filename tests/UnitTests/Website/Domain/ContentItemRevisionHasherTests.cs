using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentItemRevisionHasherTests
{
    [Fact]
    public void ComputeHash_SameInput_ProducesSameHash()
    {
        var first = ContentItemRevisionHasher.ComputeHash("{\"a\":1}");
        var second = ContentItemRevisionHasher.ComputeHash("{\"a\":1}");

        Assert.Equal(first, second);
    }

    [Fact]
    public void ComputeHash_DifferentInput_ProducesDifferentHash()
    {
        var first = ContentItemRevisionHasher.ComputeHash("{\"a\":1}");
        var second = ContentItemRevisionHasher.ComputeHash("{\"a\":2}");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void ComputeHash_ReturnsLowercaseHex64Characters()
    {
        var hash = ContentItemRevisionHasher.ComputeHash("anything");

        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9a-f]{64}$", hash);
    }
}
