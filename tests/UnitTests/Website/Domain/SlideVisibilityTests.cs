using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SlideVisibilityTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    private static Slide Build(bool isActive, DateTime? publishAtUtc, DateTime? unpublishAtUtc) =>
        Slide.Create(
            Guid.NewGuid(), null, LinkTarget.CreateEmpty(), 1, isActive, publishAtUtc, unpublishAtUtc,
            [SlideTranslation.Create(Tr, null, "Başlık", null, null, null).Value]).Value;

    [Fact]
    public void Evaluate_WhenActiveWithNoSchedule_ReturnsTrue()
    {
        var slide = Build(true, null, null);

        Assert.True(SlideVisibility.Evaluate(slide, Now));
    }

    [Fact]
    public void Evaluate_WhenInactive_ReturnsFalseRegardlessOfSchedule()
    {
        var slide = Build(false, Now.AddDays(-1), Now.AddDays(1));

        Assert.False(SlideVisibility.Evaluate(slide, Now));
    }

    [Fact]
    public void Evaluate_WhenPublishAtInTheFuture_ReturnsFalse()
    {
        var slide = Build(true, Now.AddHours(1), null);

        Assert.False(SlideVisibility.Evaluate(slide, Now));
    }

    [Fact]
    public void Evaluate_WhenPublishAtInThePast_ReturnsTrue()
    {
        var slide = Build(true, Now.AddHours(-1), null);

        Assert.True(SlideVisibility.Evaluate(slide, Now));
    }

    [Fact]
    public void Evaluate_WhenUnpublishAtInThePast_ReturnsFalse()
    {
        var slide = Build(true, null, Now.AddHours(-1));

        Assert.False(SlideVisibility.Evaluate(slide, Now));
    }

    [Fact]
    public void Evaluate_WhenUnpublishAtInTheFuture_ReturnsTrue()
    {
        var slide = Build(true, null, Now.AddHours(1));

        Assert.True(SlideVisibility.Evaluate(slide, Now));
    }

    [Fact]
    public void Evaluate_WhenWithinPublishWindow_ReturnsTrue()
    {
        var slide = Build(true, Now.AddHours(-1), Now.AddHours(1));

        Assert.True(SlideVisibility.Evaluate(slide, Now));
    }
}
