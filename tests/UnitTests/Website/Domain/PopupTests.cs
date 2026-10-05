using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class PopupTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);

    private static Result<Popup> Create(
        PopupDisplayMode displayMode = PopupDisplayMode.Modal,
        Guid? imageMediaId = null,
        LinkTarget? linkTarget = null,
        PopupTargeting? targeting = null,
        int delaySeconds = 0,
        PopupFrequency frequency = PopupFrequency.EveryVisit,
        int? frequencyDays = null,
        bool dismissible = false,
        int priority = 50,
        DateTime? publishAtUtc = null,
        DateTime? unpublishAtUtc = null,
        string? title = "Başlık",
        string? body = "<p>İçerik</p>",
        string? buttonLabel = null) =>
        Popup.Create(
            displayMode, imageMediaId, linkTarget ?? LinkTarget.CreateEmpty(), targeting ?? PopupTargeting.CreateAllPages(),
            PopupDeviceTarget.All, publishAtUtc, unpublishAtUtc, delaySeconds, frequency, frequencyDays, dismissible, priority, Tr, title,
            body, buttonLabel, UserId, Now);

    [Fact]
    public void Create_Modal_WithValidInput_Succeeds()
    {
        var result = Create();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Dismissible);
    }

    [Fact]
    public void Create_Banner_WithImage_Fails()
    {
        var result = Create(displayMode: PopupDisplayMode.Banner, imageMediaId: Guid.NewGuid(), title: null, body: "Kısa metin");

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.BannerCannotHaveImage", result.Error.Code);
    }

    [Fact]
    public void Create_Banner_WithNonZeroDelay_Fails()
    {
        var result = Create(displayMode: PopupDisplayMode.Banner, delaySeconds: 5, title: null, body: "Kısa metin");

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.BannerCannotHaveDelay", result.Error.Code);
    }

    [Fact]
    public void Create_Banner_ZeroDelayNoImage_Succeeds()
    {
        var result = Create(displayMode: PopupDisplayMode.Banner, delaySeconds: 0, title: null, body: "Kısa metin", dismissible: true);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_Banner_DismissibleAsGiven()
    {
        var result = Create(displayMode: PopupDisplayMode.Banner, title: null, body: "Kısa metin", dismissible: false);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.Dismissible);
    }

    [Fact]
    public void Create_Modal_ForcesDismissibleTrueRegardlessOfInput()
    {
        var result = Create(dismissible: false);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Dismissible);
    }

    [Fact]
    public void Create_Modal_DelayOutOfRange_Fails()
    {
        var result = Create(delaySeconds: 61);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.DelaySecondsInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_Modal_WithoutTitle_Fails()
    {
        var result = Create(title: null);

        Assert.True(result.IsFailure);
        Assert.Equal("PopupTranslation.TitleRequired", result.Error.Code);
    }

    [Fact]
    public void Create_Banner_WithoutTitle_Succeeds()
    {
        var result = Create(displayMode: PopupDisplayMode.Banner, title: null, body: "Kısa metin");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Translations.Single().Title);
    }

    [Fact]
    public void Create_Banner_BodyWithHtml_Fails()
    {
        var result = Create(displayMode: PopupDisplayMode.Banner, title: null, body: "<b>metin</b>");

        Assert.True(result.IsFailure);
        Assert.Equal("PopupTranslation.BannerBodyMustBePlainText", result.Error.Code);
    }

    [Fact]
    public void Create_Banner_BodyTooLong_Fails()
    {
        var result = Create(displayMode: PopupDisplayMode.Banner, title: null, body: new string('a', 301));

        Assert.True(result.IsFailure);
        Assert.Equal("PopupTranslation.BannerBodyTooLong", result.Error.Code);
    }

    [Fact]
    public void Create_WithoutBody_Fails()
    {
        var result = Create(body: null);

        Assert.True(result.IsFailure);
        Assert.Equal("PopupTranslation.BodyRequired", result.Error.Code);
    }

    [Fact]
    public void Create_EveryNDays_WithoutFrequencyDays_Fails()
    {
        var result = Create(frequency: PopupFrequency.EveryNDays);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.FrequencyDaysRequired", result.Error.Code);
    }

    [Fact]
    public void Create_EveryNDays_WithFrequencyDaysOutOfRange_Fails()
    {
        var result = Create(frequency: PopupFrequency.EveryNDays, frequencyDays: 400);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.FrequencyDaysRequired", result.Error.Code);
    }

    [Fact]
    public void Create_EveryNDays_WithValidFrequencyDays_Succeeds()
    {
        var result = Create(frequency: PopupFrequency.EveryNDays, frequencyDays: 7);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_NotEveryNDays_WithFrequencyDaysSet_Fails()
    {
        var result = Create(frequency: PopupFrequency.EveryVisit, frequencyDays: 7);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.FrequencyDaysNotAllowed", result.Error.Code);
    }

    [Fact]
    public void Create_UnpublishAtBeforePublishAt_Fails()
    {
        var result = Create(publishAtUtc: Now, unpublishAtUtc: Now.AddHours(-1));

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.UnpublishMustBeAfterPublish", result.Error.Code);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Create_PriorityOutOfRange_Fails(int priority)
    {
        var result = Create(priority: priority);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.PriorityInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithButtonLabelButNoLinkTarget_Fails()
    {
        var result = Create(buttonLabel: "Git");

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.ButtonLabelRequiresLink", result.Error.Code);
    }

    [Fact]
    public void Create_WithButtonLabelAndLinkTarget_Succeeds()
    {
        var link = LinkTarget.ForExternalUrl("https://example.com").Value;

        var result = Create(linkTarget: link, buttonLabel: "Git");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Update_SwitchingToBannerWithExistingImage_Fails()
    {
        var popup = Create(imageMediaId: Guid.NewGuid()).Value;

        var result = popup.Update(
            PopupDisplayMode.Banner, Guid.NewGuid(), LinkTarget.CreateEmpty(), PopupTargeting.CreateAllPages(), PopupDeviceTarget.All, null,
            null, 0, PopupFrequency.EveryVisit, null, true, 50, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.BannerCannotHaveImage", result.Error.Code);
    }

    [Fact]
    public void Update_SwitchingToBannerClearingImage_Succeeds()
    {
        // Banner rejects HTML in Body, and switching modes re-validates every existing translation
        // against the new mode - so this popup needs an already-plain-text body to isolate the image
        // rule being tested here from the (separately tested) banner-body-must-be-plain-text rule.
        var popup = Create(imageMediaId: Guid.NewGuid(), body: "Düz metin").Value;

        var result = popup.Update(
            PopupDisplayMode.Banner, null, LinkTarget.CreateEmpty(), PopupTargeting.CreateAllPages(), PopupDeviceTarget.All, null, null, 0,
            PopupFrequency.EveryVisit, null, true, 50, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(PopupDisplayMode.Banner, popup.DisplayMode);
        // Switching modes re-validates existing translations against the new mode's rules but does not
        // clear fields that are still valid under it - Title just stops being REQUIRED for Banner.
        Assert.Equal("Başlık", popup.Translations.Single().Title);
    }

    [Fact]
    public void SetTranslation_NewLanguageWithButtonLabelButNoLink_Fails()
    {
        var popup = Create().Value;
        var en = LanguageCode.Create("en").Value;

        var result = popup.SetTranslation(en, "Title", "<p>Body</p>", "Go", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.ButtonLabelRequiresLink", result.Error.Code);
    }

    [Fact]
    public void RemoveTranslation_DefaultLanguage_Fails()
    {
        var popup = Create().Value;

        var result = popup.RemoveTranslation(Tr, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.CannotDeleteDefaultTranslation", result.Error.Code);
    }
}
