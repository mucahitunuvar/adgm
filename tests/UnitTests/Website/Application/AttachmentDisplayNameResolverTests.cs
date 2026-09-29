using GenclikMerkezi.Modules.Website.Application.Media;

namespace GenclikMerkezi.UnitTests.Website.Application;

public class AttachmentDisplayNameResolverTests
{
    [Fact]
    public void Resolve_WithOverride_ReturnsOverride()
    {
        var result = AttachmentDisplayNameResolver.Resolve("Özel ad", "original.pdf");

        Assert.Equal("Özel ad", result);
    }

    [Fact]
    public void Resolve_WithoutOverride_ReturnsOriginalFileName()
    {
        var result = AttachmentDisplayNameResolver.Resolve(null, "original.pdf");

        Assert.Equal("original.pdf", result);
    }

    [Fact]
    public void Resolve_WithBlankOverride_FallsBackToOriginalFileName()
    {
        var result = AttachmentDisplayNameResolver.Resolve("   ", "original.pdf");

        Assert.Equal("original.pdf", result);
    }
}
